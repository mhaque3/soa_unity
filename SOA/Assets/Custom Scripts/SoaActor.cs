using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaActor : MonoBehaviour 
{

    public int unique_id;
    public Affiliation affiliation;
    public int type;

    public enum CarriedResource
    {
        NONE = 0,
        SUPPLIES = 1,
        CIVILIANS = 2,
        CASUALTIES = 3
    };

    public enum ActorType
    {
        //BASE = 0, // Blue base is no longer a SoaActor, it is now a SoaSite
        SMALL_UAV = 1,
        HEAVY_LIFT = 2,
        DISMOUNT = 3,
        TRUCK = 4,
        POLICE = 5,
        BALLOON = 6
    };

    public bool isAlive;
    public CarriedResource isCarrying;
    public bool isWeaponized;

    public double commsRange;

    public SoaSensor[] Sensors;
    public SoaWeapon[] Weapons;
    public List<GameObject> Detections;
    public List<GameObject> Tracks;

    public List<Belief_Actor> killDetections = new List<Belief_Actor>();

    public DataManager dataManager;
    public bool simulateMotion;
    public bool useExternalWaypoint;
    public bool displayTruePosition = true;

    public Vector3 displayPosition;
    private Quaternion displayOrientation;

    private bool useGhostModel = false;
    private bool displayActor = true;

    public Dictionary<int, bool> classificationDictionary;

    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;

    private static System.DateTime epoch = new System.DateTime(1970, 1, 1);

    public SoldierWaypointMotion motionScript;
    public NavMeshAgent navAgent;
    
    // Use this for initialization
	public virtual void Start () 
    {
        // Alive at the beginning
        isAlive = true;

        // Initially carrying nothing
        isCarrying = CarriedResource.NONE;

        displayPosition = transform.position;
        displayOrientation = transform.rotation;

        Sensors = transform.GetComponentsInChildren<SoaSensor>();

        foreach (SoaSensor sensor in Sensors)
        {
            //sensor.soaActor = this;
        }

        // Look at my children and populate list of weapons
        Weapons = transform.GetComponentsInChildren<SoaWeapon>();

        // An actor is weaponized if it has a weapon with at least one enabled mode
        foreach(SoaWeapon tempWeapon in Weapons){
            foreach (WeaponModality tempModality in tempWeapon.modes)
            {
                if (tempModality.enabled)
                {
                    isWeaponized = true;
                    break;
                }
            }
            if (isWeaponized)
            {
                break;
            }
        }

        // Get references to my motion and nav scripts
        motionScript = gameObject.GetComponent<SoldierWaypointMotion>();
        navAgent = gameObject.GetComponent<NavMeshAgent>();

        // Initialize the belief dictionary
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

        // Initialize a new classification dictionary
        classificationDictionary = new Dictionary<int, bool>();
	}

    // Update is called once per frame
    void Update() 
    {
        /*
        // Debug output 
        if (unique_id == 100)
        {
            Debug.Log("**************************************");
            foreach (Belief b in beliefDictionary[Belief.BeliefType.ACTOR].Values)
            {
                Belief_Actor a = (Belief_Actor)b;
                Debug.Log("ID: " + a.getUnique_id() + ", AFFILIATION: " + a.getAffiliation() + ", ISALIVE: " + a.getIsAlive());
            }
        }
        */
	}

    // Called when the actor has been killed
    public virtual void Kill()
    {
        // If remote platform (uses external waypoint), send a final message
        if (this.useExternalWaypoint)
        {
            // Convert position from Unity to km for Belief_Actor
            Belief_Actor newActorData = new Belief_Actor(unique_id, (int)affiliation, type, isAlive, (int)isCarrying, isWeaponized,
                transform.position.x / SimControl.KmToUnity,
                transform.position.y / SimControl.KmToUnity,
                transform.position.z / SimControl.KmToUnity);

            beliefDictionary[Belief.BeliefType.ACTOR][unique_id] = newActorData;
            if (dataManager != null)
                dataManager.addAndBroadcastBelief(newActorData, unique_id);
        }

        // Set that it is no longer alive
        isAlive = false;

        // Don't move anymore
        simulateMotion = false;
        navAgent.ResetPath();
        navAgent.enabled = false;
        motionScript.enabled = false;

        // TODO: Make sure dead unit does not act as relay for comms

        // Change appearance to look destroyed
        Vector3 destroyedPos = transform.position;
        destroyedPos.y = 0.85f;
        transform.position = destroyedPos;
    }

    //Set this actors position and orientation
    public virtual void LateUpadte()
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
    public virtual void updateActor()
    {
        if (!isAlive)
        {
            return;
        }

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
                //Debug.Log(name);
                motionScript.On = false;
                if (beliefDictionary[Belief.BeliefType.WAYPOINT].TryGetValue(unique_id, out newBelief))
                {
                    // Received a waypoint in km, transform to Unity coordinates and then use it to set the nav agent's destination
                    Belief_Waypoint newWaypoint = (Belief_Waypoint)newBelief;
                    navAgent.SetDestination(new Vector3(
                        newWaypoint.getPos_x() * SimControl.KmToUnity,
                        newWaypoint.getPos_y() * SimControl.KmToUnity,
                        newWaypoint.getPos_z() * SimControl.KmToUnity));
                    //Debug.Log("Actor " + unique_id + " has external waypoint " + newWaypoint.getPos_x() + " " + newWaypoint.getPos_y() + " " + newWaypoint.getPos_z());
                }
                else
                {
                    navAgent.SetDestination(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                }
            }
            else
            {
                // Convert position coordinates from unity to km before making new belief waypoint
                Belief_Waypoint newWaypoint = new Belief_Waypoint((ulong)(System.DateTime.UtcNow - epoch).Milliseconds, unique_id,
                    motionScript.targetPosition.x / SimControl.KmToUnity,
                    motionScript.targetPosition.y / SimControl.KmToUnity,
                    motionScript.targetPosition.z / SimControl.KmToUnity);
                beliefDictionary[Belief.BeliefType.WAYPOINT][unique_id] = newWaypoint;
                if(dataManager != null)
                    dataManager.addAndBroadcastBelief(newWaypoint, unique_id);
            }

            // Convert position from Unity to km for Belief_Actor
            Belief_Actor newActorData = new Belief_Actor(unique_id, (int)affiliation, type, isAlive, (int)isCarrying, isWeaponized,
                transform.position.x / SimControl.KmToUnity,
                transform.position.y / SimControl.KmToUnity,
                transform.position.z / SimControl.KmToUnity);

            beliefDictionary[Belief.BeliefType.ACTOR][unique_id] = newActorData;
            if(dataManager != null)
                dataManager.addAndBroadcastBelief(newActorData, unique_id);
            
            // Go through each detected object's Soa Actor, get unique ID, affiliation, and pos.  Broadcast belief out to everyone
            foreach (GameObject gameObject in Detections)
            {
                SoaActor soaActor = gameObject.GetComponent<SoaActor>();
                
                // Actor's position must be converted from Unity to km when generating belief
                Belief_Actor detectedActor;
                if (classificationDictionary.ContainsKey(soaActor.unique_id) && classificationDictionary[soaActor.unique_id])
                {
                    // I have classified this actor before, provide actual affiliation and isWeaponized info
                    detectedActor = new Belief_Actor(soaActor.unique_id, (int)soaActor.affiliation,
                        soaActor.type, soaActor.isAlive, (int)soaActor.isCarrying, soaActor.isWeaponized,
                        gameObject.transform.position.x / SimControl.KmToUnity,
                        gameObject.transform.position.y / SimControl.KmToUnity,
                        gameObject.transform.position.z / SimControl.KmToUnity);
                }
                else
                {
                    // I have never classified this actor before, set as unclassified and give default isWeaponized info
                    detectedActor = new Belief_Actor(soaActor.unique_id, (int)Affiliation.UNCLASSIFIED,
                        soaActor.type, soaActor.isAlive, (int)soaActor.isCarrying, false,
                        gameObject.transform.position.x / SimControl.KmToUnity,
                        gameObject.transform.position.y / SimControl.KmToUnity,
                        gameObject.transform.position.z / SimControl.KmToUnity);
                }
                beliefDictionary[Belief.BeliefType.ACTOR][soaActor.unique_id] = detectedActor;
                if (dataManager != null)
                {
                    dataManager.addAndBroadcastBelief(detectedActor, unique_id);
                }
            }

            //TODO make this thread safe since collisions are done by collider in a separate thread????
            foreach (Belief_Actor belief_actor in killDetections)
            {
                dataManager.addAndBroadcastBelief(new soa.Belief_Actor(
            belief_actor.getId(), (int)belief_actor.getAffiliation(), belief_actor.getType(), false, 0, belief_actor.getIsWeaponized(),
            belief_actor.getPos_x(),
            belief_actor.getPos_y(),
            belief_actor.getPos_z()), unique_id);
            }
            killDetections.Clear();

            // Having made detections, now go and update each weapon
            foreach (SoaWeapon w in Weapons)
            {
                w.UpdateWeapon(Detections);
            }

            // Finished processing Detections list, clear it
            Detections.Clear();

            useGhostModel = false;           
        }
    }

    public bool checkClassified(int uniqueId)
    {
        return classificationDictionary.ContainsKey(uniqueId) && classificationDictionary[uniqueId];
    }

    public void setClassified(int uniqueId)
    {
        // Set classification dictionary value to true if not already
        if (!checkClassified(uniqueId))
        {
            classificationDictionary[uniqueId] = true;
        }
    }

    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public virtual void addBelief(Belief b)
    {
        #if(UNITY_STANDALONE)
            //Debug.Log("SoaActor - DataManager: Received belief of type " + (int)b.getBeliefType() + "\n" + b);
        #else
        Console.WriteLine("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);
        #endif

        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        if (tempTypeDict != null)
        {
            Belief oldBelief;
            if (!beliefDictionary[b.getBeliefType()].TryGetValue(b.getId(), out oldBelief) || oldBelief.getBeliefTime() < b.getBeliefTime())
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
        //Broadcast types ACTOR, MODE_COMMAND, SPOI, WAYPOINT, WAYPOINT_OVERRIDE, BASE, NGOSITE, VILLAGE
        publishBeliefsOfType(Belief.BeliefType.ACTOR);
        publishBeliefsOfType(Belief.BeliefType.MODE_COMMAND);
        publishBeliefsOfType(Belief.BeliefType.SPOI);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT_OVERRIDE);
        publishBeliefsOfType(Belief.BeliefType.BASE);
        publishBeliefsOfType(Belief.BeliefType.NGOSITE);
        publishBeliefsOfType(Belief.BeliefType.VILLAGE);
    }

    private void publishBeliefsOfType(Belief.BeliefType type)
    {
        // Only publish beliefs if still alive
        if(isAlive){
            ulong currentTime = (ulong)(System.DateTime.UtcNow - epoch).Milliseconds;
            if (beliefDictionary.ContainsKey(type))
            {
                SortedDictionary<int, Belief> typeDict = beliefDictionary[type];
                foreach (KeyValuePair<int, Belief> entry in typeDict)
                {
                    /*if (type == Belief.BeliefType.NGOSITE)
                    {
                        Debug.Log("NGO " + entry.Value.getId() + "!!!!!!!!!!!!!!!!!!!!!!!!");
                        Debug.Log("Belief Time " + entry.Value.getBeliefTime() + " " + entry.Value.ToString());
                    }*/

                    // only publish new data (beliefs created within last 5 seconds)
                    if (entry.Value.getBeliefTime() >= (UInt64)(System.DateTime.UtcNow - epoch).Ticks/10000 - 5000)
                    {
                        /*if (type == Belief.BeliefType.NGOSITE)
                        {
                            Debug.Log("In!");
                        }*/

                        if(dataManager != null)
                            dataManager.addAndBroadcastBelief(entry.Value, unique_id);
                    }
                }
            }
        }
    }

    // Get current time
    // Note: This is a very large number.  When comparing (subtracting) times, do all math in
    // ulong and then cast at end to float as needed.  Do NOT cast to float before subtracting.
    public ulong getCurrentTime_ms()
    {
        return (ulong)(System.DateTime.UtcNow - epoch).Ticks / 10000;
    }
}
