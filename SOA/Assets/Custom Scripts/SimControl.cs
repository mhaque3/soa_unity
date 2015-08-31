using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using soa;
using Gamelogic.Grids;

public enum Affiliation { BLUE = 0, RED = 1, NEUTRAL = 2 , UNCLASSIFIED = 3 };

public class SimControl : MonoBehaviour 
{
    // Config
    string ConfigFileName = "SoaSimConfig.xml";
    
    // Prefabs
    public GameObject RedDismountPrefab;
    public GameObject RedTruckPrefab;
    public GameObject NeutralDismountPrefab;
    public GameObject NeutralTruckPrefab;
    public GameObject BluePolicePrefab;
    public GameObject HeavyUAVPrefab;
    public GameObject SmallUAVPrefab;
    //public GameObject BalloonPrefab;
    
    // GameObject Lists
    public List<GameObject> LocalPlatforms;
    public List<GameObject> RemotePlatforms;
    public List<GameObject> NgoSites;
    public List<GameObject> Villages;
    public List<GameObject> RedBases;
    public List<GameObject> BlueBases;
    public List<GridCell> MountainCells;
    public List<GridCell> WaterCells;

    // Unique IDs
    HashSet<int> TakenUniqueIDs;
    int smallestAvailableUniqueID = 200;
    
    // Conversion Factor
    static public float KmToUnity;
 
    // Misc
    public bool BroadcastOn;
    public float updateRateS;
    private bool showTruePositions = true;
    public OverheadMouseCamera omcScript;
    public SoaHexWorld hexGrid;
    DataManager redDataManager;
    DataManager blueDataManager;
    public Canvas uiCanvas;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    Vector3 labelPosition;
    Text[] labels;
    Camera thisCamera;

    // Only access this when you have the DataManager lock
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayBeliefDictionary;

    // Initialization function
	void Start () 
    {
        // Reset all existing local and remote configs as having unique id -1
        // Note: This must be called before LoadConfigFile();
        foreach (GameObject g in LocalPlatforms)
        {
            g.GetComponent<SoaActor>().unique_id = -1;
        }
        foreach (GameObject g in RemotePlatforms)
        {
            g.GetComponent<SoaActor>().unique_id = -1;
        }

        // Set up book keeping for assigning unique IDs
        // Note: This must be called before LoadConfigFile();
        TakenUniqueIDs = new HashSet<int>();
        TakenUniqueIDs.Add(0); // Reserved for blue base
        smallestAvailableUniqueID = 1;

        // Load settings from config file (should be the first thing that's called)
        LoadConfigFile();

        // Scale factor
        KmToUnity = hexGrid.KmToUnity();
        Debug.Log("Km to Unity = " + KmToUnity);

        // Set up mountain and water cells
        WaterCells = new List<GridCell>();
        MountainCells = new List<GridCell>();

        Debug.Log(hexGrid.WaterHexes.Count + " water hexes to copy");
        foreach (FlatHexPoint point in hexGrid.WaterHexes)
        {
            WaterCells.Add(new GridCell(point.Y, point.X));
        }
        Debug.Log(hexGrid.MountainHexes.Count + " mountain hexes to copy");
        foreach (FlatHexPoint point in hexGrid.MountainHexes)
        {
            MountainCells.Add(new GridCell(point.Y, point.X));
        }

        // Camera
        thisCamera = omcScript.GetComponent<Camera>();

        // NavMesh
        NavMesh.pathfindingIterationsPerFrame = 50;

        // Create manager
        displayBeliefDictionary = blueDataManager.getGodsEyeView();

        // Local IDs must be resolved after remote IDs
        for (int i = 0; i < LocalPlatforms.Count; i++)
        {
            GameObject platform = LocalPlatforms[i];
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            if (platform.name.Contains("Blue Base"))
            {
                // ID 0 is reserved for the blue base
                actor.unique_id = 0;
            }
            else
            {
                // Only those platforms not originally in LocalPlatforms list need to request an ID
                if (actor.unique_id < 0)
                {
                    actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
                }
            }

            if (platform.name.Contains("Blue"))
            {
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("Red"))
            {
                actor.useExternalWaypoint = false;
                actor.dataManager = redDataManager;
                redDataManager.addNewActor(actor);
            }
        }

        // Remote IDs must be resolved before local IDs
        Debug.Log("Adding remote platforms");
        for (int i = 0; i < RemotePlatforms.Count; i++)
        {
            GameObject platform = RemotePlatforms[i];
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            // Only those platforms not originally in RemotePlatforms list need to request an ID
            if (actor.unique_id < 0)
            {
                actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
            }

            actor.simulateMotion = true;

            if (platform.name.Contains("Blue"))
            {
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("HeavyLift"))
            {
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("SmallUav"))
            {
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("Red"))
            {
                actor.dataManager = redDataManager;
                actor.useExternalWaypoint = false;
                redDataManager.addNewActor(actor);
            }

            if (platform.name.Contains("Truck"))
            {
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Dismount"))
            {
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Police"))
            {
                actor.useExternalWaypoint = false;
            }
        }

        PushInitialMapBeliefs();

        // Last thing to do is to start comms with all beliefs in data
        // manager already initialized
        redDataManager.startComms();
        blueDataManager.startComms();
	} // End Start()
	
	// Update is called once per frame

    int randDraw;
    Belief b;
    float messageTimer = 0f;
    float updateTimer = 0f;

    float sensorClock = 0f;
    public float sensorUpdatePeriod;
	void Update () 
    {
        float dt = Time.deltaTime;

        UpdateMouseOver();

        sensorClock += dt;
        if (sensorClock > sensorUpdatePeriod)
        {
            sensorClock = 0f;

            foreach (GameObject seeker in LocalPlatforms)
            {
                seeker.BroadcastMessage("CheckDetections", LocalPlatforms, SendMessageOptions.DontRequireReceiver);
                seeker.BroadcastMessage("CheckDetections", RemotePlatforms, SendMessageOptions.DontRequireReceiver);
            }
            foreach (GameObject seeker in RemotePlatforms)
            {
                seeker.BroadcastMessage("CheckDetections", LocalPlatforms, SendMessageOptions.DontRequireReceiver);
                seeker.BroadcastMessage("CheckDetections", RemotePlatforms, SendMessageOptions.DontRequireReceiver);
            }
        }

        updateTimer += dt;
        if (updateTimer > updateRateS)
        {
            blueDataManager.calcualteDistances();
            redDataManager.calcualteDistances();
            //TODO get display parameters

            //This lock keeps the comms manager from adding data while we are reading
            lock (redDataManager.dataManagerLock)
            {
                for (int i = 0; i < LocalPlatforms.Count; i++)
                {
                    GameObject platform = LocalPlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();
                    if (actor.affiliation == Affiliation.RED)
                    {

                        //If showing true position, first arg gets ignored
                        //Otherwise, the data in the first arg is represented in the display.
                        //If null the actor is no longer visible.
                        actor.updateActor();
                    }
                }
            }
            
            lock(blueDataManager.dataManagerLock)
            {
                displayBeliefDictionary = blueDataManager.getGodsEyeView();
                for (int i = 0; i < LocalPlatforms.Count; i++)
                {
                    GameObject platform = LocalPlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();
                    if (actor.affiliation == Affiliation.BLUE)
                    {

                        //If showing true position, first arg gets ignored
                        //Otherwise, the data in the first arg is represented in the display.
                        //If null the actor is no longer visible.
                        actor.updateActor();
                    }
                }

                //Iterate over remote platform and update their positions.
                //Assume all remote platforms have blue affilition
                for (int i = 0; i < RemotePlatforms.Count; i++)
                {
                    GameObject platform = RemotePlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();

                    //If showing true position, first arg gets ignored
                    //Otherwise, the data in the first arg is represented in the display.
                    //If null the actor is no longer visible.
                    actor.updateActor();
                }

                // Push new site beliefs into each blue base
                UpdateSiteBeliefs();

                // Clear timer
                updateTimer = 0f;
            }
        }

        if (BroadcastOn)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer > updateRateS / 2f)
            {
                //TODO create separate lists for local blue and local red so that we do not need the nested data manager locks
                //This lock keeps the comms manager from adding data while we pushing out comms
                lock (redDataManager.dataManagerLock)
                {
                    lock (blueDataManager.dataManagerLock)
                    {

                        //Get the current belief map to display.  Default is the data managers map which is the gods eye view.
                        //SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayMap = redDataManager.getGodsEyeView();

                        for (int i = 0; i < LocalPlatforms.Count; i++)
                        {
                            GameObject platform = LocalPlatforms[i];
                            SoaActor actor = platform.GetComponent<SoaActor>();
                            actor.broadcastComms();
                        }

                        for (int i = 0; i < BlueBases.Count; i++)
                        {
                            GameObject platform = BlueBases[i];
                            SoaSite site = platform.GetComponent<SoaSite>();
                            site.broadcastComms();
                        }

                        messageTimer = 0f;
                    }
                }
                //Debug.Log("*** END OUTGOING MESSAGE BLOCK ***");
            }
        }
	}

    // Gets a never before assigned Unique ID and takes suggestions
    int RequestUniqueID(int requestedID)
    {
        int assignedID;

        // Try to give the requested ID first
        if (requestedID >= smallestAvailableUniqueID && !TakenUniqueIDs.Contains(requestedID))
        {
            // Can use the requested ID, mark it as taken
            assignedID = requestedID;
            TakenUniqueIDs.Add(requestedID);
        }
        else
        {
            // Cannot use the requested ID, use the next available ID 
            assignedID = smallestAvailableUniqueID;
            TakenUniqueIDs.Add(smallestAvailableUniqueID);
        }

        // Find the next smallest available ID for the next call
        while (TakenUniqueIDs.Contains(smallestAvailableUniqueID))
        {
            smallestAvailableUniqueID++;
        }

        // Return ID to user
        return assignedID;
    }

    public void relabelLocalNeutralActor(int unique_id)
    {
        SoaActor actor;
        GameObject platform;

        // Go through each of the local platforms
        for (int i = 0; i < LocalPlatforms.Count; i++)
        {
            platform = LocalPlatforms[i];
            actor = platform.GetComponent<SoaActor>();

            // Find the red actor we want by unique id
            if (platform.name.Contains("Neutral") && actor.unique_id == unique_id)
            {
                // Change its unique ID
                actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);

                // Rename the red agent
                if(platform.name.Contains("Neutral Truck"))
                {
                    platform.name = "Neutral Truck " + actor.unique_id;
                }
                else if(platform.name.Contains("Neutral Dismount"))
                {
                    platform.name = "Neutral Dismount " + actor.unique_id;
                }

                break;
            }
        }
    }

    public void relabelLocalRedActor(int unique_id)
    {
        SoaActor actor;
        GameObject platform;

        // Go through each of the local platforms
        for (int i = 0; i < LocalPlatforms.Count; i++)
        {
            platform = LocalPlatforms[i];
            actor = platform.GetComponent<SoaActor>();

            // Find the red actor we want by unique id
            if (platform.name.Contains("Red") && actor.unique_id == unique_id)
            {
                // Remove current red actor from all lists
                redDataManager.removeActor(actor);

                // Change its unique ID
                actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);

                // Rename the red agent
                if(platform.name.Contains("Red Truck"))
                {
                    platform.name = "Red Truck " + actor.unique_id;
                }
                else if(platform.name.Contains("Red Dismount"))
                {
                    platform.name = "Red Dismount " + actor.unique_id;
                }

                // Add relabeled red actor to all lists
                redDataManager.addNewActor(actor);

                break;
            }
        }
    }

    public Vector3 mouseVector;
    void UpdateMouseOver()
    {
        mouseVector = thisCamera.ScreenToViewportPoint(Input.mousePosition);
        float mx = mouseVector.x;
        float my = mouseVector.y;
        // Don't bother with any of the checks unless mouse is over the display area...
        if (mx > 0f && mx <= 1f && my > 0f && my <= 1f)
        {
            RaycastHit hit;
            Ray ray;
            //Debug.Log(mx + ":" + my);
            // Mouse over Highlight object actions...
            int layerMask = 1 << 10;
            ray = thisCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 mouseWorldPoint = hit.point;
                Vector3 viewPortPos = thisCamera.WorldToViewportPoint(mouseWorldPoint);
                Vector3 screenPos = thisCamera.ViewportToScreenPoint(viewPortPos);

                if (labelInstance == null)
                {
                    //Debug.Log("ping");
                    labelInstance = (GameObject)GameObject.Instantiate(labelUI);
                    labelTransform = labelInstance.transform as RectTransform;
                    labelTransform.SetParent(uiCanvas.transform, false);
                    labels = labelInstance.GetComponentsInChildren<Text>();
                }

                if (hit.collider.transform.parent != null)
                {
                    GameObject thisGameObject = hit.collider.transform.parent.gameObject;
                    string thisObjectName = hit.collider.transform.parent.name;
                    if (thisObjectName != null)
                    {
                        labels[0].text = thisObjectName;
                        
                        SoaActor thisActor = thisGameObject.GetComponent<SoaActor>();
                        NavMeshAgent thisNavAgent = thisGameObject.GetComponentInChildren<NavMeshAgent>();
                        if (thisNavAgent)
                        {
                            labels[1].enabled = true;
                            //labels[1].text = (thisNavAgent.remainingDistance / KmToUnity).ToString("n2") + " km path:" + thisNavAgent.hasPath;
                            labels[1].text = thisActor.motionScript.waypoints[thisActor.motionScript.waypointIndex].name;
                        }
                        else
                        {
                            labels[1].enabled = false;
                        }

                        labelPosition = screenPos - uiCanvas.transform.position;
                        labelTransform.anchoredPosition = new Vector2(labelPosition.x, labelPosition.y + labelTransform.sizeDelta.y);
                    }
                    else
                    {
                        
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        //SelectPlatform(thisGameObject); 
                    }
                }
                else
                {

                }
            }
            else
            {
                if (labelInstance != null)
                    Object.Destroy(labelInstance);
            }
        }
    }

    // Sends out initial map beliefs through the blue data manager
    void PushInitialMapBeliefs()
    {
        GameObject g;
        FlatHexPoint currentCell;

        currentCell = new FlatHexPoint(0, 0);
        b = new Belief_GridSpec(64, 36, hexGrid.Map[currentCell].x / KmToUnity , hexGrid.Map[currentCell].z / KmToUnity, 1.0f);
        blueDataManager.addBelief(b, 0);
        blueDataManager.addInitializationBelief(b);
        Debug.Log(b.ToString());

        b = new Belief_Terrain((int)soa.Terrain.MOUNTAIN, MountainCells);
        blueDataManager.addBelief(b, 0);
        blueDataManager.addInitializationBelief(b);
        Debug.Log(MountainCells.Count + " mountain hexes.");

        b = new Belief_Terrain((int)soa.Terrain.WATER, WaterCells);
        blueDataManager.addBelief(b, 0);
        blueDataManager.addInitializationBelief(b);
        Debug.Log(WaterCells.Count + " water hexes.");

        for (int i = 0; i < BlueBases.Count; i++ )
        {
            g = BlueBases[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            BlueBaseSim s = g.GetComponent<BlueBaseSim>();
            b = new Belief_Base(i, theseCells, s.Supply);
            blueDataManager.addBelief(b, 0);
            blueDataManager.addInitializationBelief(b);
            Debug.Log(b.ToString());
        }

        for (int i = 0; i < NgoSites.Count; i++)
        {
            g = NgoSites[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            NgoSim s = g.GetComponent<NgoSim>();
            b = new Belief_NGOSite(i, theseCells, s.Supply, s.Casualties, s.Civilians);
            blueDataManager.addBelief(b, 0);
            blueDataManager.addInitializationBelief(b);
            Debug.Log(b.ToString());
        }

        for (int i = 0; i < Villages.Count; i++)
        {
            g = Villages[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            VillageSim s = g.GetComponent<VillageSim>();
            b = new Belief_Village(i, theseCells, s.Supply, s.Casualties);
            blueDataManager.addBelief(b, 0);
            blueDataManager.addInitializationBelief(b);
            Debug.Log(b.ToString());
        }
    }

    void UpdateSiteBeliefs()
    {
        GameObject g;
        FlatHexPoint currentCell;

        currentCell = new FlatHexPoint(0, 0);

        for (int i = 0; i < BlueBases.Count; i++)
        {
            g = BlueBases[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            BlueBaseSim s = g.GetComponent<BlueBaseSim>();
            b = new Belief_Base(i, theseCells, s.Supply);
            AddBeliefToBlueBases(b);
            //Debug.Log(b.ToString());
        }

        for (int i = 0; i < NgoSites.Count; i++)
        {
            g = NgoSites[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            NgoSim s = g.GetComponent<NgoSim>();
            b = new Belief_NGOSite(i, theseCells, s.Supply, s.Casualties, s.Civilians);
            AddBeliefToBlueBases(b);
            //Debug.Log(b.ToString());
        }

        for (int i = 0; i < Villages.Count; i++)
        {
            g = Villages[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            VillageSim s = g.GetComponent<VillageSim>();
            b = new Belief_Village(i, theseCells, s.Supply, s.Casualties);
            AddBeliefToBlueBases(b);
            //Debug.Log(b.ToString());
        }
    }

    void AddBeliefToBlueBases(Belief b)
    {
        GameObject g;
        for (int i = 0; i < BlueBases.Count; i++)
        {
            g = BlueBases[i];
            SoaSite s = g.GetComponent<SoaSite>();
            s.addBelief(b);
        }
    }

    public void AddLocalPlatform(GameObject newPlatform)
    {
        LocalPlatforms.Add(newPlatform);
        omcScript.AddPlatform(newPlatform);
    }

    public void AddRemotePlatform(GameObject newPlatform)
    {
        RemotePlatforms.Add(newPlatform);
        omcScript.AddPlatform(newPlatform);
    }

    void BroadcastPlatforms()
    {
        foreach (GameObject platform in LocalPlatforms)
        {
            Debug.Log("Sending platform " + platform.name);
        }
    }

    void OnApplicationQuit()
    {
        redDataManager.stopPhoton();
        blueDataManager.stopPhoton();
    }

    GameObject FindClosestInList(GameObject g, List<GameObject> targetList)
    {
        GameObject closestTarget = null;
        float closestDist = float.PositiveInfinity;
        float tempDist;
        foreach (GameObject t in targetList)
        {
            tempDist = (g.transform.position - t.transform.position).magnitude;
            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                closestTarget = t;
            }
        }
        return closestTarget;
    }

    // Reads in the XML config file
    void LoadConfigFile()
    {
        // Parse the XML config file
        SoaConfig soaConfig = SoaConfigXMLReader.Parse(ConfigFileName);

        // Set up networking
        redDataManager = new DataManager(soaConfig.networkRedRoom);
        blueDataManager = new DataManager(soaConfig.networkBlueRoom);

        // Set up remote platforms
        foreach (PlatformConfig p in soaConfig.remotePlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.HEAVY_UAV:
                    {
                        // Instantiate
                        HeavyUAVConfig c = (HeavyUAVConfig)p;
                        GameObject g = (GameObject)Instantiate(HeavyUAVPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "HeavyLift " + a.unique_id;

                        // Add to list of remote platforms
                        RemotePlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.SMALL_UAV:
                    {
                        // Instantiate
                        SmallUAVConfig c = (SmallUAVConfig)p;
                        GameObject g = (GameObject)Instantiate(SmallUAVPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "SmallUAV " + a.unique_id;

                        // Add to list of remote platforms
                        RemotePlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.BALLOON:
                    {
                        /*BalloonConfig c = (BalloonConfig) p;
                        GameObject g = (GameObject)Instantiate(BalloonPrefab, c.pos, Quaternion.identity);
                        
                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;
                         
                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Balloon " + a.unique_id;
                         
                        // Add to list of remote platforms
                        RemotePlatforms.Add(g);*/
                        break;
                    }
            }
        }

        // Set up local platforms
        foreach (PlatformConfig p in soaConfig.localPlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.RED_DISMOUNT:
                    {
                        // Instantiate
                        RedDismountConfig c = (RedDismountConfig)p;
                        GameObject g = (GameObject)Instantiate(RedDismountPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Red Dismount " + a.unique_id;

                        // Waypoint Motion
                        SoldierWaypointMotion swm = g.GetComponent<SoldierWaypointMotion>();
                        GameObject waypoint;
                        bool initialWaypointSpecified = false;
                        if (c.initialWaypoint != null)
                        {
                            waypoint = GameObject.Find(c.initialWaypoint);
                            if (waypoint != null)
                            {
                                // Initial waypoint was specified, go to it first and then
                                // go to the closest red base from there
                                initialWaypointSpecified = true;
                                swm.waypoints.Add(waypoint);
                                swm.waypoints.Add(FindClosestInList(waypoint, RedBases));
                            }
                        }
                        if (!initialWaypointSpecified)
                        {
                            // Go to current closest base if no initial waypoint was specified
                            swm.waypoints.Add(FindClosestInList(g, RedBases));
                        }

                        // Weapon
                        SoaWeapon sw = g.GetComponentInChildren<SoaWeapon>();
                        foreach (WeaponModality wm in sw.modes)
                        {
                            wm.enabled = c.hasWeapon;
                        }

                        // Add to list of local platforms
                        LocalPlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.RED_TRUCK:
                    {
                        // Instantiate
                        RedTruckConfig c = (RedTruckConfig)p;
                        GameObject g = (GameObject)Instantiate(RedTruckPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Red Truck " + a.unique_id;

                        // Waypoint Motion
                        SoldierWaypointMotion swm = g.GetComponent<SoldierWaypointMotion>();
                        GameObject waypoint;
                        bool initialWaypointSpecified = false;
                        if (c.initialWaypoint != null)
                        {
                            waypoint = GameObject.Find(c.initialWaypoint);
                            if (waypoint != null)
                            {
                                // Initial waypoint was specified, go to it first and then
                                // go to the closest red base from there
                                initialWaypointSpecified = true;
                                swm.waypoints.Add(waypoint);
                                swm.waypoints.Add(FindClosestInList(waypoint, RedBases));
                            }
                        }
                        if (!initialWaypointSpecified)
                        {
                            // Go to current closest base if no initial waypoint was specified
                            swm.waypoints.Add(FindClosestInList(g, RedBases));
                        }

                        // Weapon
                        SoaWeapon sw = g.GetComponentInChildren<SoaWeapon>();
                        foreach (WeaponModality wm in sw.modes)
                        {
                            wm.enabled = c.hasWeapon;
                        }

                        // Add to list of local platforms
                        LocalPlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.NEUTRAL_DISMOUNT:
                    {
                        // Instantiate
                        NeutralDismountConfig c = (NeutralDismountConfig)p;
                        GameObject g = (GameObject)Instantiate(NeutralDismountPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Neutral Dismount " + a.unique_id;

                        // Add to list of local platforms & explicitly start
                        LocalPlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.NEUTRAL_TRUCK:
                    {
                        // Instantiate
                        NeutralTruckConfig c = (NeutralTruckConfig)p;
                        GameObject g = (GameObject)Instantiate(NeutralTruckPrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Neutral Truck " + a.unique_id;

                        // Add to list of local platforms
                        LocalPlatforms.Add(g);
                        break;
                    }
                case PlatformConfig.ConfigType.BLUE_POLICE:
                    {
                        // Instantiate
                        BluePoliceConfig c = (BluePoliceConfig)p;
                        GameObject g = (GameObject)Instantiate(BluePolicePrefab, c.pos, Quaternion.identity);

                        // Track on Grid
                        TrackOnGrid t = g.GetComponent<TrackOnGrid>();
                        t.hexGrid = hexGrid;

                        // Unique ID
                        SoaActor a = g.GetComponent<SoaActor>();
                        a.unique_id = RequestUniqueID(c.id);
                        g.name = "Blue Police " + a.unique_id;

                        // Add to list of local platforms
                        LocalPlatforms.Add(g);
                        break;
                    }
            }
        }
    }
}
