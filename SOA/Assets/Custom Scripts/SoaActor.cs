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
    public bool simulateMotion = true;
    public bool displayTruePosition = true;

    private Vector3 displayPosition;
    private Quaternion displayOrientation;

    private Vector3 truePosition;
    private Quaternion trueOrientation;

    private bool useGhostModel = false;
    private bool displayActor = true;

    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;
	// Use this for initialization
	void Start () 
    {
        displayPosition = transform.position;
        displayOrientation = transform.rotation;

        truePosition = transform.position;
        trueOrientation = transform.rotation;

        Sensors = transform.GetComponentsInChildren<SoaSensor>();

        foreach (SoaSensor sensor in Sensors)
        {
            sensor.soaActor = this;
        }
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
                transform.position = truePosition;
                transform.rotation = trueOrientation;
            }
            else
            {
                transform.position = displayPosition;
                transform.rotation = displayOrientation;
            }

            if (useGhostModel)
            {
                //TODO set ghost model visible
            }
            else
            {
                //TODO set real model visible
            }
        }
        else
        {
            //TODO set model not visisble
        }

    }

    /**
     * Update the position of the actor vectors.  If this is a simulation instance (simulateMotion = true)
     * then simulate the next position.
     * Otherwise set the display position vectors to the Belief_Actor position.
     * Check if displaying old data and choose the appropriate model.
     * 
     * Set whether or not to display the true position of the agent
     */ 
    public void updateActor(Belief_Actor myActor, bool displayTruePosition)
    {
        this.displayTruePosition = displayTruePosition;

        if (myActor != null && myActor.getId() != unique_id)
        {
            Debug.LogError("Update Actor " + unique_id + " has invalid id " + myActor.getId());
            return;
        }

        if (simulateMotion)
        {
            displayActor = true;
            //TODO Update the true position based on the waypoint for this actor
            
            Belief_Actor newActorData = new Belief_Actor(unique_id, myActor.getAffiliation(), myActor.getType(), truePosition.x, truePosition.y, truePosition.z);
            beliefDictionary[Belief.BeliefType.ACTOR][unique_id] = newActorData;
            
            useGhostModel = false;
            
        }
        else if (myActor != null)
        {
            displayActor = true;
            displayPosition = new Vector3(myActor.getPos_x(), myActor.getPos_y(), myActor.getPos_z());

            //TODO Check age of belief, if belief is older than 5 seconds, switch to ghost model, otherwise use normal model
            useGhostModel = true;
        }
        else
        {
            displayActor = false;
        }
                


    }

    void setSimulateMotion(bool simulateMotion)
    {
        this.simulateMotion = simulateMotion;
    }


    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public void addBelief(Belief b)
    {
#if(NOT_UNITY)
            Console.WriteLine("DataManager: Received belief of type "
                + (int)b.getBeliefType() + "\n" + b);
#else
        Debug.Log("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);

        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        if (tempTypeDict != null)
        {
            Belief oldBelief = beliefDictionary[b.getBeliefType()][b.getId()];
            if (oldBelief == null || oldBelief.getTime() < b.getTime())
            {
                beliefDictionary[b.getBeliefType()][b.getId()] = b;
            }
        }
        else
        {
            beliefDictionary[b.getBeliefType()][b.getId()] = b;
        }
#endif
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
        ulong currentTime = 2222222222222222;//  timeManager.getCurrentTime();
        SortedDictionary<int, Belief> typeDict = beliefDictionary[type];
        foreach(KeyValuePair<int, Belief> entry in typeDict)
        {
            //only publish new data
            if (entry.Value.getTime() < currentTime - 5000)
            {
                dataManager.addAndBroadcastBelief(entry.Value, entry.Key);
            }
        }
    }

}
