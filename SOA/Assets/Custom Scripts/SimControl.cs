﻿using UnityEngine;
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
    int smallestAvailableUniqueID = 200; // Start assigning IDs from here
    
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
    public Vector3 mouseVector;

    // Config file parameters
    public string networkRedRoom;
    public string networkBlueRoom;
    public float probRedTruckWeaponized;
    public float probRedDismountWeaponized;

    // For updates
    int randDraw;
    Belief b;
    float messageTimer = 0f;
    float updateTimer = 0f;
    float sensorClock = 0f;
    public float sensorUpdatePeriod;

    // Only access this when you have the DataManager lock
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayBeliefDictionary;

    #region Initialization
    /*****************************************************************************************************
     *                                        INITIALIZATION                                             *
     *****************************************************************************************************/

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

        // Create data managers
        redDataManager = new DataManager(networkRedRoom);
        blueDataManager = new DataManager(networkBlueRoom);

        // Get gods eye view
        displayBeliefDictionary = blueDataManager.getGodsEyeView();

        // Activate local platforms (both pre-existing and instantiated from config)
        for (int i = 0; i < LocalPlatforms.Count; i++)
        {
            GameObject platform = LocalPlatforms[i];
            if (platform.tag.Contains("BlueBase"))
            {
                ActivateBlueBase(platform);
            }
            else if (platform.tag.Contains("RedDismount"))
            {
                ActivateRedDismount(platform);
            }
            else if (platform.tag.Contains("RedTruck"))
            {
                ActivateRedTruck(platform);
            }
            else if (platform.tag.Contains("NeutralDismount"))
            {
                ActivateNeutralDismount(platform);
            }
            else if (platform.tag.Contains("NeutralTruck"))
            {
                ActivateNeutralTruck(platform);
            }
            else if (platform.tag.Contains("BluePolice"))
            {
                ActivateBluePolice(platform);
            }
            else
            {
                Debug.LogWarning("Error activating local platform, unrecognized tag " + platform.tag);
            }
        }

        // Activate remote platforms (both pre-existing and instantiated from config)
        for (int i = 0; i < RemotePlatforms.Count; i++)
        {
            GameObject platform = RemotePlatforms[i];
            if (platform.tag.Contains("HeavyUAV"))
            {
                ActivateHeavyUAV(platform);
            }
            else if (platform.tag.Contains("SmallUAV"))
            {
                ActivateSmallUAV(platform);
            }
            else if (platform.tag.Contains("Balloon"))
            {
                ActivateBalloon(platform);
            }
            else
            {
                Debug.LogWarning("Error activating remote platform, unrecognized tag " + platform.tag);
            }
        }

        // Add map beliefs to outgoing queue
        PushInitialMapBeliefs();

        // Last thing to do is to start comms with all beliefs in data
        // manager already initialized
        redDataManager.startComms();
        blueDataManager.startComms();
	} // End Start()

    // Reads in the XML config file
    void LoadConfigFile()
    {
        // Parse the XML config file
        SoaConfig soaConfig = SoaConfigXMLReader.Parse(ConfigFileName);

        // Red platform weapon probability
        probRedTruckWeaponized = soaConfig.probRedTruckWeaponized;
        probRedDismountWeaponized = soaConfig.probRedDismountWeaponized;

        // Network settings
        networkRedRoom = soaConfig.networkRedRoom;
        networkBlueRoom = soaConfig.networkBlueRoom;

        // Set up remote platforms
        foreach (PlatformConfig p in soaConfig.remotePlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.HEAVY_UAV:
                    InstantiateHeavyUAV((HeavyUAVConfig)p);
                    break;
                case PlatformConfig.ConfigType.SMALL_UAV:
                    InstantiateSmallUAV((SmallUAVConfig)p);
                    break;
                case PlatformConfig.ConfigType.BALLOON:
                    Debug.LogWarning("Balloon creation currently disabled");
                    //InstantiateBalloon((BalloonConfig) p);
                    break;
            }
        }

        // Set up local platforms
        foreach (PlatformConfig p in soaConfig.localPlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.RED_DISMOUNT:
                    InstantiateRedDismount((RedDismountConfig)p);
                    break;
                case PlatformConfig.ConfigType.RED_TRUCK:
                    InstantiateRedTruck((RedTruckConfig)p);
                    break;
                case PlatformConfig.ConfigType.NEUTRAL_DISMOUNT:
                    InstantiateNeutralDismount((NeutralDismountConfig)p);
                    break;
                case PlatformConfig.ConfigType.NEUTRAL_TRUCK:
                    InstantiateNeutralTruck((NeutralTruckConfig)p);
                    break;
                case PlatformConfig.ConfigType.BLUE_POLICE:
                    InstantiateBluePolice((BluePoliceConfig)p);
                    break;
            }
        }
    }

    // Sends out initial map beliefs through the blue data manager
    void PushInitialMapBeliefs()
    {
        GameObject g;
        FlatHexPoint currentCell;

        currentCell = new FlatHexPoint(0, 0);
        b = new Belief_GridSpec(64, 36, hexGrid.Map[currentCell].x / KmToUnity, hexGrid.Map[currentCell].z / KmToUnity, 1.0f);
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

        for (int i = 0; i < BlueBases.Count; i++)
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
    #endregion

    #region Update
    /*****************************************************************************************************
     *                                              UPDATE                                               *
     *****************************************************************************************************/
    // Update is called once per frame
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
            }
        }
	}

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

    void UpdateSiteBeliefs()
    {
        GameObject g;
        FlatHexPoint currentCell = new FlatHexPoint(0, 0);

        for (int i = 0; i < BlueBases.Count; i++)
        {
            g = BlueBases[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            BlueBaseSim s = g.GetComponent<BlueBaseSim>();
            b = new Belief_Base(i, theseCells, s.Supply);
            AddBeliefToBlueBases(b);
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
        }
    }
    #endregion

    #region Termination
    /*****************************************************************************************************
     *                                           TERMINATION                                             *
     *****************************************************************************************************/
    void OnApplicationQuit()
    {
        redDataManager.stopPhoton();
        blueDataManager.stopPhoton();
    }
    #endregion

    #region Utility Functions
    /*****************************************************************************************************
     *                                        UTILITY FUNCTIONS                                          *
     *****************************************************************************************************/
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
    #endregion

    #region Remote Platform Instantiation
    /*****************************************************************************************************
     *                                  REMOTE PLATFORM INSTANTIATION                                    *
     *****************************************************************************************************/
    public GameObject InstantiateHeavyUAV(HeavyUAVConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(HeavyUAVPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "HeavyLift " + a.unique_id;

        // Add to list of remote platforms
        RemotePlatforms.Add(g);
        return g;
    }

    public GameObject InstantiateSmallUAV(SmallUAVConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(SmallUAVPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;
        
        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "SmallUAV " + a.unique_id;

        // Add to list of remote platforms
        RemotePlatforms.Add(g);
        return g;
    }

    public GameObject InstantiateBalloon(BalloonConfig c)
    {
        // Instantiate
        /*GameObject g = (GameObject)Instantiate(BalloonPrefab, c.pos, Quaternion.identity);
                        
        // Set grid
        g.GetComponent<TrackOnGrid>.hexGrid = hexGrid;
                         
        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Balloon " + a.unique_id;
                         
        // Add to list of remote platforms
        RemotePlatforms.Add(g);
        return g;*/

        return null;
    }
    #endregion

    #region Remote Platform Activation
    /*****************************************************************************************************
     *                                    REMOTE PLATFORM ACTIVATION                                     *
     *****************************************************************************************************/
    private void ActivateRemotePlatform(GameObject platform)
    {
        // Add to mouse script
        omcScript.AddPlatform(platform);

        // Assign unique ID if does not already have one
        SoaActor actor = platform.GetComponent<SoaActor>();
        if (actor.unique_id < 0)
        {
            actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
        }

        // Settings
        actor.simulateMotion = true;
        actor.useExternalWaypoint = true;

        // Data manager
        actor.dataManager = blueDataManager;
        blueDataManager.addNewActor(actor);
    }

    public void ActivateHeavyUAV(GameObject platform)
    {
        ActivateRemotePlatform(platform);
    }

    public void ActivateSmallUAV(GameObject platform)
    {
        ActivateRemotePlatform(platform);
    }

    public void ActivateBalloon(GameObject platform)
    {
        ActivateRemotePlatform(platform);
    }
    #endregion

    #region Local Platform Instantiation
    /*****************************************************************************************************
     *                                   LOCAL PLATFORM INSTANTIATION                                    *
     *****************************************************************************************************/
    public GameObject InstantiateRedDismount(RedDismountConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(RedDismountPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Red Dismount " + a.unique_id;

        // Waypoint motion
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
        return g;
    }

    public GameObject InstantiateRedTruck(RedTruckConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(RedTruckPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Red Truck " + a.unique_id;

        // Waypoint motion
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
        return g;
    }

    public GameObject InstantiateNeutralDismount(NeutralDismountConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(NeutralDismountPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Neutral Dismount " + a.unique_id;

        // Add to list of local platforms
        LocalPlatforms.Add(g);
        return g;
    }

    public GameObject InstantiateNeutralTruck(NeutralTruckConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(NeutralTruckPrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Neutral Truck " + a.unique_id;

        // Add to list of local platforms
        LocalPlatforms.Add(g);
        return g;
    }

    public GameObject InstantiateBluePolice(BluePoliceConfig c)
    {
        // Instantiate
        GameObject g = (GameObject)Instantiate(BluePolicePrefab, c.pos, Quaternion.identity);

        // Set grid
        g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

        // Assign unique ID
        SoaActor a = g.GetComponent<SoaActor>();
        a.unique_id = RequestUniqueID(c.id);
        g.name = "Blue Police " + a.unique_id;

        // Add to list of local platforms
        LocalPlatforms.Add(g);
        return g;
    }
    #endregion

    #region Local Platform Activation
    /*****************************************************************************************************
     *                                     LOCAL PLATFORM ACTIVATION                                     *
     *****************************************************************************************************/
    private void ActivateBlueBase(GameObject platform)
    {
        // Add to mouse script
        omcScript.AddPlatform(platform);

        // ID 0 is reserved for blue base
        SoaActor actor = platform.GetComponent<SoaActor>();
        actor.unique_id = 0;

        // Data manager
        actor.dataManager = blueDataManager;
        blueDataManager.addNewActor(actor);
    }

    private void ActivateLocalRedPlatform(GameObject platform)
    {
        // Add to mouse script
        omcScript.AddPlatform(platform);

        // Assign unique ID if does not already have one
        SoaActor actor = platform.GetComponent<SoaActor>();
        if (actor.unique_id < 0)
        {
            actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
        }

        // Settings
        actor.useExternalWaypoint = false;

        // Data manager
        actor.dataManager = redDataManager;
        redDataManager.addNewActor(actor);
    }

    public void ActivateRedDismount(GameObject platform)
    {
        ActivateLocalRedPlatform(platform);
    }

    public void ActivateRedTruck(GameObject platform)
    {
        ActivateLocalRedPlatform(platform);
    }

    private void ActivateLocalNeutralPlatform(GameObject platform)
    {
        // Add to mouse script
        omcScript.AddPlatform(platform);

        // Assign unique ID if does not already have one
        SoaActor actor = platform.GetComponent<SoaActor>();
        if (actor.unique_id < 0)
        {
            actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
        }
    }

    public void ActivateNeutralDismount(GameObject platform)
    {
        ActivateLocalNeutralPlatform(platform);
    }

    public void ActivateNeutralTruck(GameObject platform)
    {
        ActivateLocalNeutralPlatform(platform);
    }

    public void ActivateBluePolice(GameObject platform)
    {
        // Add to mouse script
        omcScript.AddPlatform(platform);

        // Assign unique ID if does not already have one
        SoaActor actor = platform.GetComponent<SoaActor>();
        if (actor.unique_id < 0)
        {
            actor.unique_id = RequestUniqueID(smallestAvailableUniqueID);
        }

        // Data manager
        actor.dataManager = blueDataManager;
        blueDataManager.addNewActor(actor);
    }
    #endregion

    #region Local Platform Destroy
    /*****************************************************************************************************
     *                                     LOCAL PLATFORM DESTROY                                       *
     *****************************************************************************************************/
    private void DestroyLocalRedPlatform(GameObject platform)
    {
        // Remove from mouse script
        omcScript.DeletePlatform(platform);

        // Remove from data manager
        redDataManager.removeActor(platform.GetComponent<SoaActor>());

        // Remove from local platform list
        LocalPlatforms.Remove(platform);

        // Destroy now
        Destroy(platform);
    }

    public void DestroyRedDismount(GameObject platform)
    {
        DestroyLocalRedPlatform(platform);
    }

    public void DestroyRedTruck(GameObject platform)
    {
        DestroyLocalRedPlatform(platform);
    }

    private void DestroyLocalNeutralPlatform(GameObject platform)
    {
        // Remove from mouse script
        omcScript.DeletePlatform(platform);

        // Remove from local platform list
        LocalPlatforms.Remove(platform);

        // Destroy now
        Destroy(platform);
    }

    public void DestroyNeutralDismount(GameObject platform)
    {
        DestroyLocalNeutralPlatform(platform);
    }

    public void DestroyNeutralTruck(GameObject platform)
    {
        DestroyLocalNeutralPlatform(platform);
    }
    #endregion
}
