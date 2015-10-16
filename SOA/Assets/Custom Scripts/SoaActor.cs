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

    // Simulated altitude [km]
    public float simAltitude_km;
    private float desiredAltitude_km;

    //Simulated position [km]
    protected float simX_km;
    protected float simZ_km;

    // Kinematic constraints
    public float minAltitude_km;
    public float maxAltitude_km;
    public float maxVerticalSpeed_m_s;

    private int[] idArray = new int[1];

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
        HEAVY_UAV = 2,
        DISMOUNT = 3,
        TRUCK = 4,
        POLICE = 5,
        BALLOON = 6
    };

    public bool isAlive = false;
    private bool broadcastDeathNotice = false;
    public CarriedResource isCarrying;
    public bool isWeaponized;
    public float fuelRemaining_s;
    public float commsRange;

    public SoaSensor[] Sensors;
    public SoaWeapon[] Weapons;
    public SoaJammer[] Jammers;
    public SoaClassifier[] Classifiers;
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
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> unmergedBeliefDictionary;
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> remoteBeliefs;

    private static System.DateTime epoch = new System.DateTime(1970, 1, 1);

    public SoldierWaypointMotion motionScript;
    public WaypointMotion wpMotionScript;
    public NavMeshAgent navAgent;
    SimControl simControlScript;

    private float velocityX = 0;
    private float velocityY = 0;
    private float velocityZ = 0;

    private bool velocityXValid = false;
    private bool velocityYValid = false;
    private bool velocityZValid = false;

    NavMeshAgent nma;

    // Use this for initialization
    void Start()
    {
        //Initialize id array for photon
        idArray[0] = unique_id;

        // Alive at the beginning
        isAlive = true;

        // Initially carrying nothing
        isCarrying = CarriedResource.NONE;

        displayPosition = transform.position;
        displayOrientation = transform.rotation;

        Sensors = transform.GetComponentsInChildren<SoaSensor>();

        // Look at my children and populate list of classifiers
        Classifiers = transform.GetComponentsInChildren<SoaClassifier>();

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
        wpMotionScript = gameObject.GetComponent<WaypointMotion>();
        navAgent = gameObject.GetComponent<NavMeshAgent>();

        // Save reference to simControl
        simControlScript = GameObject.FindObjectOfType<SimControl>();

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
        beliefDictionary[Belief.BeliefType.SUPPLY_DELIVERY] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();

        unmergedBeliefDictionary = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
        unmergedBeliefDictionary[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.SUPPLY_DELIVERY] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
        unmergedBeliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();

        remoteBeliefs = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
        remoteBeliefs[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.SUPPLY_DELIVERY] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
        remoteBeliefs[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();

        Debug.Log("SoaActor: Initialized all beliefDictionaries for " + gameObject.name);

        // Initialize a new classification dictionary
        classificationDictionary = new Dictionary<int, bool>();

        // Initialize initial altitude to be average of min and max altitudes
        simAltitude_km = 0.5f * (minAltitude_km + maxAltitude_km);
        desiredAltitude_km = simAltitude_km;

        // Set to alive now, must be last thing done
        isAlive = true;
        nma = GetComponent<NavMeshAgent>();
	}


    // Update is called once per frame
    void Update() 
    {
        if (isAlive)
        {
            // Compute time since last frame
            float dt_s = Time.deltaTime * 60; // Time.deltaTime returns sec, recall 1 sec real time = 60 sec in sim world
            
            // Simulate altitude state
            if (simAltitude_km != desiredAltitude_km)
            {
                float diffAltitude_km = desiredAltitude_km - simAltitude_km;
                if (Mathf.Abs(diffAltitude_km) <= maxVerticalSpeed_m_s / 1000.0f * dt_s)
                {
                    // We can achieve the desired altitude in this time step
                    simAltitude_km = desiredAltitude_km;
                }
                else if (desiredAltitude_km > simAltitude_km)
                {
                    // Move max speed up
                    simAltitude_km += maxVerticalSpeed_m_s / 1000.0f * dt_s;
                }
                else
                {
                    // Move max speed down
                    simAltitude_km -= maxVerticalSpeed_m_s / 1000.0f * dt_s;
                }
            }

            // Decrement fuel accordingly
            // Note: Fuel is measured in seconds of simulated time, not real time
            fuelRemaining_s = Mathf.Max(0.0f, fuelRemaining_s - dt_s);
            if (fuelRemaining_s <= 0)
            {
                // Killed by insufficient fuel
                Kill("Insufficient Fuel");
            }
        }
	}

    // Set simulated altitude but enforce min/max constraints
    public void SetSimAltitude(float simAltitude_km)
    {
        this.simAltitude_km = Mathf.Max(Mathf.Min(simAltitude_km, maxAltitude_km), minAltitude_km);
    }

    // Set desired altitude but enforce min/max constraints
    public void SetDesiredAltitude(float desiredAltitude_km)
    {
        this.desiredAltitude_km = Mathf.Max(Mathf.Min(desiredAltitude_km, maxAltitude_km), minAltitude_km);
    }

    // Called when the actor has been killed
    public virtual void Kill(string killerName)
    {
        if (isAlive)
        {
            // Set that it is no longer alive
            isAlive = false;

            // Log event
            if (gameObject.CompareTag("HeavyUAV"))
            {
                simControlScript.soaEventLogger.LogHeavyUAVLost(gameObject.name, killerName);
            }
            else if (gameObject.CompareTag("SmallUAV"))
            {
                simControlScript.soaEventLogger.LogSmallUAVLost(gameObject.name, killerName);
            }
            

                // Convert position from Unity to km for Belief_Actor
                Belief_Actor newActorData = new Belief_Actor(unique_id, (int)affiliation, 
                    type, isAlive, (int)isCarrying, isWeaponized, fuelRemaining_s,
                    transform.position.x / SimControl.KmToUnity,
                    simAltitude_km,
                    transform.position.z / SimControl.KmToUnity);

                addMyBeliefData(newActorData);
                //addBeliefToBeliefDictionary(newActorData);
                //addBelief(newActorData, remoteBeliefs);


            // Don't move anymore
            simulateMotion = false;
            if (navAgent != null)
            {
                navAgent.ResetPath();
                navAgent.enabled = false;
            }
            if (motionScript != null)
            {
                motionScript.enabled = false;
            }

            // TODO: Make sure dead unit does not act as relay for comms

            // Change appearance to look destroyed
            Vector3 destroyedPos = transform.position;
            destroyedPos.y = 0.85f;
            transform.position = destroyedPos;
        }
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

        if (nma != null)
        {
            velocityX = (nma.velocity.x / SimControl.KmToUnity) / 60f * 1000f;
            velocityXValid = true;
            velocityY = (nma.velocity.y / SimControl.KmToUnity) / 60f * 1000f;
            velocityYValid = true;
            velocityZ = (nma.velocity.z / SimControl.KmToUnity) / 60f * 1000f;
            velocityZValid = true;

            //Debug.Log("VELOCITY " + unique_id + " " + (nma.velocity.magnitude / 60f * 1000f));
        }

        if (simulateMotion)
        {
            displayActor = true;

            // Setup waypoint belief
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
                        newWaypoint.getPos_y() * SimControl.KmToUnity, // Nav agent ignores the y coordinate (altitude)
                        newWaypoint.getPos_z() * SimControl.KmToUnity));

                    // Set the desired altitude separately [km]
                    SetDesiredAltitude(newWaypoint.getPos_y());
                }
                else
                {
                    // Stay put
                    navAgent.SetDestination(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                }
            }
            else if (motionScript != null)
            {
                // Convert position coordinates from unity to km before making new belief waypoint
                Belief_Waypoint newWaypoint = new Belief_Waypoint((ulong)(System.DateTime.UtcNow - epoch).Milliseconds, unique_id,
                    motionScript.targetPosition.x / SimControl.KmToUnity,
                    desiredAltitude_km,
                    motionScript.targetPosition.z / SimControl.KmToUnity);
                addMyBeliefData(newWaypoint);
            }
            else
            {
                // Special case for blue balloon
                // Convert position coordinates from unity to km before making new belief waypoint
                Belief_Waypoint newWaypoint = new Belief_Waypoint((ulong)(System.DateTime.UtcNow - epoch).Milliseconds, unique_id,
                    wpMotionScript.targetPosition.x / SimControl.KmToUnity,
                    desiredAltitude_km,
                    wpMotionScript.targetPosition.z / SimControl.KmToUnity);
                addMyBeliefData(newWaypoint);
            }

            // Convert position from Unity to km for Belief_Actor
            simX_km = transform.position.x / SimControl.KmToUnity;
            simZ_km = transform.position.z / SimControl.KmToUnity;

            Belief_Actor newActorData = new Belief_Actor(unique_id, (int)affiliation, 
                type, isAlive, (int)isCarrying, isWeaponized, fuelRemaining_s,
                simX_km,
                simAltitude_km,
                simZ_km,
                velocityXValid,
                velocityX,
                velocityYValid,
                velocityY,
                velocityZValid,
                velocityZ);

            addMyBeliefData(newActorData);
            if (dataManager != null)
                dataManager.addBeliefToDataManager(newActorData, unique_id);
            
            // Update classifications
            foreach (SoaClassifier c in Classifiers)
            {
                c.UpdateClassifications(Detections);
            }

            // Update each weapon
            foreach (SoaWeapon w in Weapons)
            {
                w.UpdateWeapon(Detections);
            }

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
                        soaActor.type, soaActor.isAlive, (int)soaActor.isCarrying, soaActor.isWeaponized, soaActor.fuelRemaining_s,
                        gameObject.transform.position.x / SimControl.KmToUnity,
                        soaActor.simAltitude_km,
                        gameObject.transform.position.z / SimControl.KmToUnity);
                }
                else
                {
                    // I have never classified this actor before, set as unclassified and give default isWeaponized info
                    detectedActor = new Belief_Actor(soaActor.unique_id, (int)Affiliation.UNCLASSIFIED,
                        soaActor.type, soaActor.isAlive, (int)soaActor.isCarrying, false, soaActor.fuelRemaining_s,
                        gameObject.transform.position.x / SimControl.KmToUnity,
                        soaActor.simAltitude_km,
                        gameObject.transform.position.z / SimControl.KmToUnity);
                }

                addMyBeliefData(detectedActor);
                
                
            }
            Detections.Clear();

            //TODO make this thread safe since collisions are done by collider in a separate thread????
            foreach (Belief_Actor belief_actor in killDetections)
            {

                addMyBeliefData(new soa.Belief_Actor(
                    belief_actor.getId(), (int)belief_actor.getAffiliation(), 
                    belief_actor.getType(), false, 0, 
                    belief_actor.getIsWeaponized(), belief_actor.getFuelRemaining(),
                    belief_actor.getPos_x(),
                    belief_actor.getPos_y(),
                    belief_actor.getPos_z()));

            }
            killDetections.Clear();

            // ???
            useGhostModel = false;           
        }
    }

    //Add data from the sensors of this actor (position updates, sensor data).  This goes to the belief dictionary and the remote beliefs to be sent.
    private void addMyBeliefData(Belief b)
    {
        
        if(addBelief(b, beliefDictionary))
        {
            
           bool addedToRemote = addBelief(b, remoteBeliefs) ;
           
        }
    }

    public bool checkClassified(int uniqueId)
    {
        return classificationDictionary.ContainsKey(uniqueId) && classificationDictionary[uniqueId];
    }

    public void setClassified(int uniqueId)
    {
        // Set classification dictionary value to true if not already
        classificationDictionary[uniqueId] = true;
    }

    public bool addBeliefToBeliefDictionary(Belief b)
    {
        return addBelief(b, beliefDictionary);
    }

    /*
     * Add belief to the unmerged map
     */ 
    public bool addBeliefToUnmergedBeliefDictionary(Belief b)
    {
        return addBelief(b, unmergedBeliefDictionary);
    }

    public void mergeBeliefDictionary()
    {
        foreach (KeyValuePair<Belief.BeliefType, SortedDictionary<int, Belief>> entry in unmergedBeliefDictionary)
        {
            foreach(KeyValuePair<int, Belief> beliefs in entry.Value)
            {
                if(addBelief(beliefs.Value, beliefDictionary))
                {
                    addBelief(beliefs.Value, remoteBeliefs);
                }
            }
        }
    }

    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public virtual bool addBelief(Belief b, SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> beliefDictionary)
    {
        #if(UNITY_STANDALONE)
            //Debug.Log("SoaActor - DataManager: Received belief of type " + (int)b.getBeliefType() + "\n" + b);
        #else
        Console.WriteLine("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);
        #endif

        // Get the dictionary for that belief type
        Belief.BeliefType bt = b.getBeliefType();
        try
        {
            int i = beliefDictionary.Count;
        }
        catch (Exception)
        {
            Debug.LogWarning("SoaActor: Exception from beliefDictionary for " + gameObject.name);
        }
        
        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        bool updateDictionary;
        Belief oldBelief;
        if (tempTypeDict != null && beliefDictionary[b.getBeliefType()].TryGetValue(b.getId(), out oldBelief))
        {
            // We are in here if a previous belief already exists and we have to merge
            if (b.getBeliefType() == Belief.BeliefType.ACTOR)
            {
                // Convert to belief actors
                Belief_Actor oldActorBelief = (Belief_Actor)oldBelief;
                Belief_Actor incomingActorBelief = (Belief_Actor)b;

                // To keep track of what to merge
                bool useIncomingClassification = false;
                bool useIncomingData = false;

                // Check which classification to use
                if (oldActorBelief.getAffiliation() == (int)Affiliation.UNCLASSIFIED &&
                    incomingActorBelief.getAffiliation() != (int)Affiliation.UNCLASSIFIED)
                {
                    // Incoming belief has new classification information
                    useIncomingClassification = true;
                }

                // Check which data to use
                if (incomingActorBelief.getBeliefTime() > oldActorBelief.getBeliefTime())
                {
                    // Incoming belief has new data information
                    useIncomingData = true;
                }

                // Merge based on what was new
                if (!useIncomingClassification && !useIncomingData)
                {
                    // No new classification or new data, just ignore the incoming belief
                    updateDictionary = false;
                }
                else if (!useIncomingClassification && useIncomingData)
                {
                    // Keep existing classification and just take incoming data
                    updateDictionary = true;
                    b = new Belief_Actor(
                        incomingActorBelief.getUnique_id(),
                        oldActorBelief.getAffiliation(),
                        incomingActorBelief.getType(),
                        incomingActorBelief.getIsAlive(),
                        incomingActorBelief.getIsCarrying(),
                        oldActorBelief.getIsWeaponized(),
                        incomingActorBelief.getFuelRemaining(),
                        incomingActorBelief.getPos_x(),
                        incomingActorBelief.getPos_y(),
                        incomingActorBelief.getPos_z(),
                        incomingActorBelief.getVelocity_x_valid(),
                        incomingActorBelief.getVelocity_x(),
                        incomingActorBelief.getVelocity_y_valid(),
                        incomingActorBelief.getVelocity_y(),
                        incomingActorBelief.getVelocity_z_valid(),
                        incomingActorBelief.getVelocity_z());
                    b.setBeliefTime(incomingActorBelief.getBeliefTime());
                }
                else if (useIncomingClassification && !useIncomingData)
                {
                    // Use incoming classification but keep existing data
                    updateDictionary = true;
                    b = new Belief_Actor(
                        oldActorBelief.getUnique_id(),
                        incomingActorBelief.getAffiliation(),
                        oldActorBelief.getType(),
                        oldActorBelief.getIsAlive(),
                        oldActorBelief.getIsCarrying(),
                        incomingActorBelief.getIsWeaponized(),
                        oldActorBelief.getFuelRemaining(),
                        oldActorBelief.getPos_x(),
                        oldActorBelief.getPos_y(),
                        oldActorBelief.getPos_z(),
                        oldActorBelief.getVelocity_x_valid(),
                        oldActorBelief.getVelocity_x(),
                        oldActorBelief.getVelocity_y_valid(),
                        oldActorBelief.getVelocity_y(),
                        oldActorBelief.getVelocity_z_valid(),
                        oldActorBelief.getVelocity_z());
                    b.setBeliefTime(oldActorBelief.getBeliefTime());
                }
                else
                {
                    // Use all of the incoming belief
                    updateDictionary = true;
                    b = incomingActorBelief;
                }
            }
            else
            {
                // General merge policy (take newest belief) for every belief except actor  
                if (oldBelief.getBeliefTime() < b.getBeliefTime())
                {
                    updateDictionary = true;
                }
                else
                {
                    updateDictionary = false;
                }
            }
        }
        else
        {
            // Nothing in the dictionary for this belief type, put new entry in
            updateDictionary = true;
        }

        // Update the dictionary entry if necessary
        //broadcast update to remote data manager
        if (updateDictionary)
        {
            //TODO find out which photon comm manager this agent is listening on and only send to that one

            //dataManager.broadcastBelief(b,unique_id, null);
            beliefDictionary[b.getBeliefType()][b.getId()] = b;
        }

        return updateDictionary;
    }

    public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> getBeliefDictionary()
    {
        return beliefDictionary;
    }


    //Broadcast new beliefs that have been added this update cycle and then clear them from remotebeliefs after they have been sent
    public void updateRemoteAgent()
    {
        foreach (KeyValuePair<Belief.BeliefType, SortedDictionary<int, Belief>> entry in remoteBeliefs)
        {
            foreach (KeyValuePair<int, Belief> beliefEntry in entry.Value)
            {
                dataManager.broadcastBelief(beliefEntry.Value, unique_id, idArray);
            }
            entry.Value.Clear();
        }
    }


    public virtual void broadcastCommsLocal()
    {
        if (dataManager == null) return;
        List<SoaActor> connectedActors = new List<SoaActor>();
        SortedDictionary<int, bool> actorCommDictionary;
        if (dataManager.actorDistanceDictionary.TryGetValue(unique_id, out actorCommDictionary))
        {
            foreach (KeyValuePair<int, bool> entry in actorCommDictionary)
            {
                if (entry.Value)
                {
                    SoaActor neighborActor;
                    if (dataManager.soaActorDictionary.TryGetValue(entry.Key, out neighborActor))
                    {
                        connectedActors.Add(neighborActor);
                    }
                }
            }
            localBroadcastBeliefsOfType(Belief.BeliefType.ACTOR, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.MODE_COMMAND, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.SPOI, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.SUPPLY_DELIVERY, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.WAYPOINT, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.WAYPOINT_OVERRIDE, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.NGOSITE, connectedActors);
            localBroadcastBeliefsOfType(Belief.BeliefType.VILLAGE, connectedActors);
        }
        else
        {
            Debug.LogError("[SoaActor.BroadcastLocalComms] Actor " + unique_id + "not in dictionary");
        }
    }

    //
    public Vector3 getPositionVector_km()
    {
        return new Vector3(simX_km, simAltitude_km, simZ_km);
    }

    protected void localBroadcastBeliefsOfType(Belief.BeliefType type, List<SoaActor> connectedActors)
    {
        if (beliefDictionary.ContainsKey(type))
            {
                SortedDictionary<int, Belief> typeDict = beliefDictionary[type];
                foreach (KeyValuePair<int, Belief> entry in typeDict)
                {
                    foreach(SoaActor actor in connectedActors)
                    {
                        actor.addBeliefToUnmergedBeliefDictionary(entry.Value);
                    }
                }
            }
        
    }

    public void broadcastComms_old()
    {
        //Broadcast types ACTOR, MODE_COMMAND, SPOI, WAYPOINT, WAYPOINT_OVERRIDE, BASE, NGOSITE, VILLAGE
        publishBeliefsOfType_old(Belief.BeliefType.ACTOR);
        publishBeliefsOfType_old(Belief.BeliefType.MODE_COMMAND);
        publishBeliefsOfType_old(Belief.BeliefType.SPOI);
        publishBeliefsOfType_old(Belief.BeliefType.SUPPLY_DELIVERY);
        publishBeliefsOfType_old(Belief.BeliefType.WAYPOINT);
        publishBeliefsOfType_old(Belief.BeliefType.WAYPOINT_OVERRIDE);
        publishBeliefsOfType_old(Belief.BeliefType.BASE);
        publishBeliefsOfType_old(Belief.BeliefType.NGOSITE);
        publishBeliefsOfType_old(Belief.BeliefType.VILLAGE);
    }

    private void publishBeliefsOfType_old(Belief.BeliefType type)
    {
        // Only publish beliefs if still alive
        if(isAlive){
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
                    // TODO address protocol changes needed to accomodate classifications/is alive/ other issues tbd

                    if (entry.Value.getBeliefTime() >= (UInt64)(System.DateTime.UtcNow - epoch).Ticks/10000 - 5000)
                    {
                        /*if (type == Belief.BeliefType.NGOSITE)
                        {
                            Debug.Log("In!");
                        }*/

                        if(dataManager != null)
                            dataManager.broadcastBelief(entry.Value, unique_id, idArray);
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
