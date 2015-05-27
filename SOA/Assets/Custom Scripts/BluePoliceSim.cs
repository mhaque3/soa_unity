using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class BluePoliceSim : MonoBehaviour 
{
    // How often assignments are re-evaluated and waypoint recomputed
    public float evalPeriod_s;

    // Oldest allowed actor belief
    public float beliefTimeout_ms;

    // Misc
    int numPursueCandidates;

    // Pointer to blue base to get task assignment
    NavMeshAgent thisNavAgent;
    SimControl simControlScript;
    SoaActor thisSoaActor;

    // Local copy of belief dictionary
    SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> beliefDictionary;

    // Local copy of red units
    Dictionary<int, RedUnitInfo> redUnitDatabase;

    // Time keeping
    float timeSinceLastEval_s;
    
    // Parameters
    public float pursueRadius;

    // Variables for keeping track of task
    public enum Task
    {
        GUARD,
        PURSUE
    };
    public Task currTask;
    public int pursueActorID;
    public int numVirtualCasualties;

    // Saving unity game object references
    List<GameObject> redUnits;
    List<GameObject> protectedSites;
    List<GameObject> redBases;

    // To keep track of protected site info
    Dictionary<string, Vector3> protectedSitePos;
    Dictionary<string, float> protectedSiteVirtualCasualties;
    Dictionary<string, float> protectedSiteTotalCasualties;


    // Use this for initialization
	void Start () 
    {   
        // Initialize databases
        protectedSitePos = new Dictionary<string, Vector3>();
        protectedSiteVirtualCasualties = new Dictionary<string,float>();
        protectedSiteTotalCasualties = new Dictionary<string,float>();

        // Get and store references
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // Initial task
        currTask = Task.GUARD;

        // Red unit database
        redUnitDatabase = new Dictionary<int, RedUnitInfo>();

        // Save references to Unity game objects for quick lookup of properties
        redUnits = new List<GameObject>();
        redUnits.AddRange(GameObject.FindGameObjectsWithTag("RedTruck"));
        redUnits.AddRange(GameObject.FindGameObjectsWithTag("RedDismount"));
        protectedSites = new List<GameObject>();
        protectedSites.AddRange(GameObject.FindGameObjectsWithTag("NGO"));
        protectedSites.AddRange(GameObject.FindGameObjectsWithTag("Village"));
        redBases = new List<GameObject>(GameObject.FindGameObjectsWithTag("RedBase"));

        // Initialize protected site weights
        InitProtectedSites();
    }

    // Come up with initial weights for the protected sites
    void InitProtectedSites()
    {
        // First save the location of each base
        foreach (GameObject p in protectedSites)
        {
            protectedSitePos.Add(p.name, p.transform.position);
        }

        // Compute maxmin dist of each protected site to red bases
        Vector3 displacement;
        Dictionary<string, float> minDistToRed = new Dictionary<string, float>();
        foreach (GameObject p in protectedSites)
        {
            // Compute min distance between p and red bases
            float minDist = float.PositiveInfinity;
            foreach (GameObject r in redBases)
            {
                displacement = p.transform.position - r.transform.position;
                if (displacement.magnitude < minDist)
                {
                    minDist = displacement.magnitude;
                }
            }

            // Save min distance from p to red bases for use later
            minDistToRed.Add(p.name, minDist);
        }

        // Compute average and min of min distances to red bases
        float avgMinDist = 0.0f;
        float minMinDist = float.PositiveInfinity;
        float count = 0;
        foreach (GameObject p in protectedSites)
        {
            // Get distance to red
            float tempDist = minDistToRed[p.name];

            // Accumulate average
            avgMinDist += tempDist;
            count++;

            // Keep track of min of min
            if (tempDist < minMinDist)
            {
                minMinDist = tempDist;
            }
        }
        avgMinDist /= count;

        // Compute weights for each base
        float weightAccum = 0.0f;
        foreach (GameObject p in protectedSites)
        {
            float weight = Mathf.Exp(-1 * (minDistToRed[p.name] - minMinDist) / (avgMinDist - minMinDist));
            protectedSiteVirtualCasualties.Add(p.name, weight);
            weightAccum += weight;
        }

        // Normalize and then scale weights so that total number of virtual casualties dispersed across
        // bases equals numVirtual Casualties
        foreach (GameObject p in protectedSites)
        {
            protectedSiteVirtualCasualties[p.name] *= (numVirtualCasualties / weightAccum);
        }
    }

	// Update is called once per frame
	void Update () 
    {
        // Increment time elapsed
        timeSinceLastEval_s += Time.deltaTime;

        // Take action if update period reached
        if (timeSinceLastEval_s >= evalPeriod_s)
        {
            // Update protected site casualty count
            updateProtectedSiteCasualtyCount();

            // Get the latest belief dictionary and red unit list from SoaActor
            beliefDictionary = gameObject.GetComponent<SoaActor>().getBeliefDictionary();

            // Update red unit database
            updateRedUnitDatabase();

            // Make a note of those who are candidates for being pursued
            markPursueCandidates();

            // Assign a task
            assignTask();

            // Debug output
            /*switch (currTask)
            {
                case Task.GUARD:
                    Debug.Log("Police Task = GUARD");
                    break;
                case Task.PURSUE:
                    Debug.Log("Police Task = PURSUE " + pursueActorID);
                    break;
            }*/

            // Compute control law for that task
            thisNavAgent.SetDestination(computeControl());

            // Reset timer
            timeSinceLastEval_s = 0.0f;
        }
	}

    void updateProtectedSiteCasualtyCount()
    {
        // Update total casualty count for each protected site
        foreach (GameObject p in protectedSites)
        {
            switch (p.tag)
            {
                case "NGO":
                    protectedSiteTotalCasualties[p.name] =
                        protectedSiteVirtualCasualties[p.name] + p.GetComponent<NgoSim>().Casualties;
                    break;
                case "Village":
                    protectedSiteTotalCasualties[p.name] =
                        protectedSiteVirtualCasualties[p.name] + p.GetComponent<VillageSim>().Casualties;
                    break;
                default:
                    Debug.LogError("Unrecognized p.tag " + p.tag);
                    break;
            }
        }
    }

    // Store information on whether each red unit is carrying a civilian or not
    void updateRedUnitDatabase()
    {
        // Clear
        redUnitDatabase.Clear();

        // Loop through all actor beliefs and update the database
        foreach (Belief b in beliefDictionary[Belief.BeliefType.ACTOR].Values)
        {
            // We care only about red units
            Belief_Actor ba = (Belief_Actor)b;

            if (ba.getAffiliation().Equals((int)Affiliation.RED))
            {
                // Save the info depending on how old the belief is
                if ((float)(thisSoaActor.getCurrentTime_ms() - ba.getBeliefTime()) <= beliefTimeout_ms)
                {
                    redUnitDatabase.Add(ba.getId(), new RedUnitInfo(ba, redUnits, redBases, protectedSites));
                }
            }
        }
    }

    // Keeps track of a pursue list (really map) with hysterisis distances to closest base as condition
    // for being added/removed from map.  Each key is an actor ID for a red unit and the value is the
    // updated distance to closest base
    void markPursueCandidates()
    {
        // Clear the number of pursue candidates
        numPursueCandidates = 0;

        // Look at each red unit in the database and determine if it should be a candidate to be pursued
        foreach (int id in redUnitDatabase.Keys)
        {
            // Get the record
            RedUnitInfo redUnitInfo = redUnitDatabase[id];

            // Estimate how long it takes for me to get there
            float myTravelTime = GetRangeTo(redUnitInfo.closestRedBasePos) / thisNavAgent.speed;

            // Estimate red unit round trip time
            float redRoundTripTime;
            if (redUnitInfo.hasCivilian)
            {
                // Guess that it is going straight to closest base
                redRoundTripTime = redUnitInfo.distToClosestRedBase / redUnitInfo.maxSpeed;

                // Determine if I can get it before it returns to base (include some inefficiencies)
                if (myTravelTime < redRoundTripTime)
                {
                    // Mark as a pursue candidate and increment count
                    redUnitInfo.isPursueCandidate = true;
                    numPursueCandidates++;
                }
            }
            else
            {
                if (redUnitInfo.distToClosestProtectedSite <= pursueRadius)
                {
                    // Guess that it will first go to closest protected site and then to closest base
                    redRoundTripTime = (redUnitInfo.distToClosestProtectedSite +
                        Vector3.Distance(redUnitInfo.closestProtectedSitePos, redUnitInfo.closestRedBasePos))
                        / redUnitInfo.maxSpeed;

                    // Determine if I can get it before it returns to base (include some inefficiencies)
                    if (myTravelTime < redRoundTripTime)
                    {
                        // Mark as a pursue candidate and increment count
                        redUnitInfo.isPursueCandidate = true;
                        numPursueCandidates++;
                    }
                }
            }
        }
    }

    // Assigns a task to current police unit based on the pursueList
    public void assignTask()
    {
        if (numPursueCandidates <= 0)
        {
            // If no one to pursue then get into a guard position
            currTask = Task.GUARD;
        }
        else
        {
            // Pick one agent to pursue
            float minCost = float.PositiveInfinity;
            foreach (int id in redUnitDatabase.Keys)
            {
                // Get the record
                RedUnitInfo redUnitInfo = redUnitDatabase[id];

                // Only consider for pursuit if it has been marked as a candidate
                if (redUnitInfo.isPursueCandidate)
                {
                    // Cost is based on current distance from pursuer
                    float actorCost = GetRangeTo(redUnitInfo.pos);

                    // Those with civilians get a discount
                    if (redUnitInfo.hasCivilian)
                    {
                        actorCost *= 0.50f;
                    }

                    // Found best so far, save its score and actor ID
                    if(actorCost < minCost)
                    {
                        minCost = actorCost;
                        pursueActorID = id;
                    }
                }
            }
                        
            // Set intent to pursue
            currTask = Task.PURSUE;
        }
    }

    public Vector3 computeControl()
    {
        switch (currTask)
        {
            case Task.GUARD:
                // Guard area
                // TODO: Implement variant of Cortes/Bullo Coverage Contol Algorithm

                // Find weighted average of all protected sites
                Vector3 destination = new Vector3(0, 0, 0);
                float totalWeight = 0.0f;
                foreach (string s in protectedSitePos.Keys)
                {
                    destination += protectedSitePos[s] * protectedSiteTotalCasualties[s];
                    totalWeight += protectedSiteTotalCasualties[s];
                }
                destination /= totalWeight;
                return destination;

            case Task.PURSUE:
                // TODO: Implement motion camouflage or intercept
                // Classical pursuit for now
                return redUnitDatabase[pursueActorID].pos;
            
            default:
                // Unrecognized task
                Debug.LogError("computeControl(): Unrecognized currTask " + currTask.ToString());
                return new Vector3(float.NaN, float.NaN, float.NaN);
        }
    }

    public float GetRangeTo(GameObject destination)
    {
        return GetRangeTo(destination.transform.position);
    }

    public float GetRangeTo(Vector3 position)
    {
        NavMeshPath thisPath = new NavMeshPath();
        if (thisNavAgent.CalculatePath(position, thisPath))
        {
            return PathLength(thisPath);
        }
        else
        {
            return -1f;
        }
    }

    float PathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return 0f;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        while (i < path.corners.Length)
        {
            Vector3 currentCorner = path.corners[i];
            lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
            i++;
        }
        return lengthSoFar;
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(transform.name + " collides with " + other.name);

        if (other.CompareTag("RedTruck"))
        {
            RedTruckSim r = other.gameObject.GetComponent<RedTruckSim>();
            if (r != null)
            {
                Debug.Log(transform.name + " intercepts " + other.name);
            }
        }

        if (other.CompareTag("RedDismount"))
        {
            RedDismountSim r = other.gameObject.GetComponent<RedDismountSim>();
            if (r != null)
            {
                Debug.Log(transform.name + " intercepts " + other.name);
            }
        }

        if (other.CompareTag("NGO"))
        {
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {

            }
        }

        if (other.CompareTag("Village"))
        {
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {

            }
        }
    }
}
