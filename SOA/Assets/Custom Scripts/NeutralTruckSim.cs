using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class NeutralTruckSim : MonoBehaviour 
{
    SimControl simControlScript;
    SoldierWaypointMotion waypointScript;
    NavMeshAgent thisNavAgent;
    GameObject currDestination;
    List<GameObject> choices;
    SoaActor thisSoaActor;

    // Awake is called before anything else
    void Awake()
    {
        // Get references to scripts
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        waypointScript = gameObject.GetComponent<SoldierWaypointMotion>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // To hold source/destination choices during random selection
        choices = new List<GameObject>();
    }

	// Use this for initialization
	void Start () 
    {
        // Unlimited fuel tank
        thisSoaActor.fuelRemaining_s = float.PositiveInfinity;

        // Randomly assign a destination
        currDestination = ChooseRandomNeutralSite(true);
        OverrideWaypoint(currDestination);
	}
	
	// Update is called once per frame
    public float PathUpdateInterval;
    float PathUpdateClock = 0f;
	void Update ()
    {
        /*float dt = Time.deltaTime;
        PathUpdateClock += dt;
        if (PathUpdateClock > PathUpdateInterval)
        {
            thisNavAgent.ResetPath();
            PathUpdateClock = Random.value * PathUpdateInterval * 0.5f;
        }*/
    }

    GameObject ChooseRandomNeutralSite(bool differentFromCurrentLocation)
    {
        // Count the number of places to choose from
        choices.Clear();
        choices.AddRange(simControlScript.Villages);
        choices.AddRange(simControlScript.NgoSites);

        // Log error if we do not have at least 2 choices
        if (choices.Count < 2)
        {
            Debug.LogError("NeutralTruckSim.ChooseRandomNeutral(): # of NGO Sites + Villages must be >= 2");
        }

        // Choose sites with uniform probability (Random.Range upper limit is exclusive)
        GameObject chosen = choices[Mathf.FloorToInt(Random.Range(0.0f, (float)choices.Count))];
        if(differentFromCurrentLocation)
        {
            while((chosen.transform.position - gameObject.transform.position).magnitude == 0)
            {
                // Keep picking until we get a place that's different from current location
                chosen = choices[Mathf.FloorToInt(Random.Range(0.0f, (float)choices.Count))];
            }
        }

        return chosen;
    }

    void OnTriggerEnter(Collider other)
    {
        // Determine if we have reached our destination
        bool destinationReached = false;
        if (other.CompareTag("NGO") && other.gameObject.name.Equals(currDestination.name))
        {
            destinationReached = true;
        }
        else if (other.CompareTag("Village") && other.gameObject.name.Equals(currDestination.name))
        {
            destinationReached = true;
        }

        // If so, remove self and create a new agent at one of the sites
        if (destinationReached)
        {
            // Destroy self
            simControlScript.DestroyNeutralTruck(gameObject);

            // Configure a replacement agent
            GameObject startSite = ChooseRandomNeutralSite(false);
            NeutralTruckConfig c = new NeutralTruckConfig(
                startSite.transform.position.x / SimControl.KmToUnity,
                thisSoaActor.simAltitude_km,
                startSite.transform.position.z / SimControl.KmToUnity,
                new Optional<int>(), // id (determined at runtime)
                new Optional<float>() // beamwidth (use default)
                );

            // Instantiate and activate a replacement
            simControlScript.ActivateNeutralTruck(simControlScript.InstantiateNeutralTruck(c, true));
        }
    }

    void OverrideWaypoint(GameObject g)
    {
        waypointScript.On = false;
        thisNavAgent.ResetPath();
        waypointScript.waypointIndex = 0;
        waypointScript.waypoints.Clear();
        waypointScript.waypoints.Add(currDestination);
        waypointScript.On = true;
    }
}
