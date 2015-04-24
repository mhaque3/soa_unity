using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaActor : MonoBehaviour 
{
    public int unique_id;
    public int affiliation;
    public int type;

    public double commsRange;

    public SoaSensor[] Sensors;
    public List<GameObject> Detections;
    public List<GameObject> Tracks;

    public DataManager dataManager;
    public bool simulateMotion;
    public bool useExternalWaypoint;
    public bool displayTruePosition = true;

    private Vector3 displayPosition;
    private Quaternion displayOrientation;

    private bool useGhostModel = false;
    private bool displayActor = true;

    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;

    private static System.DateTime epoch = new System.DateTime(1970, 1, 1);

    public SoldierWaypointMotion motionScript;
    public NavMeshAgent navAgent;
	// Use this for initialization
	void Start () 
    {
        displayPosition = transform.position;
        displayOrientation = transform.rotation;

        Sensors = transform.GetComponentsInChildren<SoaSensor>();

        foreach (SoaSensor sensor in Sensors)
        {
            sensor.soaActor = this;
        }

        motionScript = gameObject.GetComponent<SoldierWaypointMotion>();
        navAgent = gameObject.GetComponent<NavMeshAgent>();

        beliefDictionary = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
        beliefDictionary[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();

	}

    // Update is called once per frame
    void Update() 
    {
	}


    //Set this actors position and orientation
    void LateUpadte()
    {
        if (displayActor)
        {
            if (displayTruePosition)
            {
                //TODO turn on real model, destroy/turn off other models
            }
            else if(!useGhostModel)
            {
                //TODO spawn substitute model, turn off real model
            }else
            {
                //TODO spawn ghost model at display position, turn off real model
            }
        }
        else
        {
            //TODO set model not visisble
        }

    }

    /**
     * Update the position of the actor vectors.  This function is called when in simulation mode
     * If Sim is in charge of waypoints, get the current target from the motion script and add to 
     * belief map.  Otherwise get the waypointBelief from the dictionary and set as navAgent destination.
     * 
     * TODO create functions for when in client mode and only reading position data
     */ 
    public void updateActor()
    {
        Belief myActor = null;
        displayTruePosition = true;

        if (myActor != null && myActor.getId() != unique_id)
        {
            Debug.LogError("Update Actor " + unique_id + " has invalid id " + myActor.getId());
            return;
        }

        if (simulateMotion)
        {
            displayActor = true;

            if (useExternalWaypoint)
            {
                Belief newBelief;
                motionScript.On = false;
                if (beliefDictionary[Belief.BeliefType.WAYPOINT].TryGetValue(unique_id, out newBelief))
                {
                    Belief_Waypoint newWaypoint = (Belief_Waypoint)newBelief;
                    navAgent.SetDestination(new Vector3(newWaypoint.getPos_x(), newWaypoint.getPos_y(), newWaypoint.getPos_z()));
                }
                else
                {
                    navAgent.SetDestination(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                }
            }
            else
            {
                float targetX = motionScript.targetPosition.x;
                float targetY = motionScript.targetPosition.y;
                float targetZ = motionScript.targetPosition.z;
                Belief_Waypoint newWaypoint = new Belief_Waypoint((ulong)(System.DateTime.UtcNow - epoch).Milliseconds, unique_id, targetX, targetY, targetZ);
                beliefDictionary[Belief.BeliefType.WAYPOINT][unique_id] = newWaypoint;
                dataManager.addAndBroadcastBelief(newWaypoint, unique_id);
            }

            Belief_Actor newActorData = new Belief_Actor(unique_id, affiliation, type, transform.position.x, transform.position.y, transform.position.z);
            beliefDictionary[Belief.BeliefType.ACTOR][unique_id] = newActorData;
            dataManager.addAndBroadcastBelief(newActorData, unique_id);
            
            useGhostModel = false;
            
        }
        
    }


    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public void addBelief(Belief b)
    {
        #if(UNITY_STANDALONE)
        Debug.Log("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);
        #else
        Console.WriteLine("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);
        #endif

        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        if (tempTypeDict != null)
        {
            Belief oldBelief = beliefDictionary[b.getBeliefType()][b.getId()];
            if (oldBelief == null || oldBelief.getBeliefTime() < b.getBeliefTime())
            {
                beliefDictionary[b.getBeliefType()][b.getId()] = b;
            }
        }
        else
        {
            beliefDictionary[b.getBeliefType()][b.getId()] = b;
        }
    }

    public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> getBeliefDictionary()
    {
        return beliefDictionary;
    }

    public void broadcastComms()
    {
        //Broadcast types ACTOR, MODE_COMMAND, SPOI, WAYPOINT, WAYPOINT_OVERRIDE
        publishBeliefsOfType(Belief.BeliefType.ACTOR);
        publishBeliefsOfType(Belief.BeliefType.MODE_COMMAND);
        publishBeliefsOfType(Belief.BeliefType.SPOI);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT_OVERRIDE);
    }

    private void publishBeliefsOfType(Belief.BeliefType type)
    {
        ulong currentTime = (ulong)(System.DateTime.UtcNow - epoch).Milliseconds;
        if (beliefDictionary.ContainsKey(type))
        {
            SortedDictionary<int, Belief> typeDict = beliefDictionary[type];
            foreach (KeyValuePair<int, Belief> entry in typeDict)
            {
                //only publish new data
                if (entry.Value.getBeliefTime() < currentTime - 5000)
                {
                    dataManager.addAndBroadcastBelief(entry.Value, entry.Key);
                }
            }
        }
    }

}
