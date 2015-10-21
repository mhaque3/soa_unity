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
    // Game duration
    public float gameDurationHr = 12.0f; // Measured in simulated time
    public float gameClockHr = 0f; // Measured in simulated time
    
    // Config
    string ConfigFileName = "SoaSimConfig.xml";
    string EnvFileName = "SoaEnvConfig.xml";

    // Prefabs
    public GameObject BlueBasePrefab;
    public GameObject RedBasePrefab;
    public GameObject NGOSitePrefab;
    public GameObject VillagePrefab;
    public GameObject RedDismountPrefab;
    public GameObject RedTruckPrefab;
    public GameObject NeutralDismountPrefab;
    public GameObject NeutralTruckPrefab;
    public GameObject BluePolicePrefab;
    public GameObject HeavyUAVPrefab;
    public GameObject SmallUAVPrefab;
    public GameObject BlueBalloonPrefab;
    
    // GameObject Lists
    public List<GameObject> LocalPlatforms;
    public List<GameObject> RemotePlatforms;
    public List<GameObject> NgoSites;
    public List<GameObject> Villages;
    public List<GameObject> RedBases;
    public List<GameObject> BlueBases;
    public List<GridCell> MountainCells;
    public List<GridCell> WaterCells;
    public List<GridCell> LandCells;

    // Unique IDs
    HashSet<int> TakenUniqueIDs;
    int smallestAvailableUniqueID = 200; // Start assigning IDs from here
    
    // Conversion Factor
    static public float KmToUnity;
    static public float gridOrigin_x;
    static public float gridOrigin_z;
    static public float gridToWorldScale;
    static public GridMath gridMath;
 
    // Logging
    public SoaEventLogger soaEventLogger;

    // Misc
    public bool BroadcastOn;
    public float updateRateS;
    private bool showTruePositions = true;
    public OverheadMouseCamera omcScript;
    public SoaHexWorld hexGrid;

    DataManager redDataManager;
    DataManager blueDataManager;
    public static List<SoaJammer> jammers = new List<SoaJammer>();

    public Canvas uiCanvas;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    Vector3 labelPosition;
    Text[] labels;
    Camera thisCamera;
    public Vector3 mouseVector;
    FlatHexPoint currentCell;

    // Config file parameters
    public SoaConfig soaConfig;
    public string networkRedRoom;
    public string networkBlueRoom;
    public float probRedTruckHasWeapon;
    public float probRedTruckHasJammer;
    public float probRedDismountHasWeapon;
    public Dictionary<string, float> defaultCommsRanges;
    public Dictionary<string, float> defaultJammerRanges;

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
        // Scale factor (this must be the first thing called)
        KmToUnity = hexGrid.KmToUnity();
        Debug.Log("Km to Unity = " + KmToUnity);

        // Initialize gridmath for grid to world conversions
        //FlatHexPoint currentCell;
        currentCell = new FlatHexPoint(0, 0);
        gridOrigin_x = hexGrid.Map[currentCell].x / KmToUnity;
        gridOrigin_z = hexGrid.Map[currentCell].z / KmToUnity;
        gridToWorldScale = 1.0f;
        gridMath = new GridMath(gridOrigin_x, gridOrigin_z, gridToWorldScale);

        // Set up mountain and water cells
        WaterCells = new List<GridCell>();
        MountainCells = new List<GridCell>();
        LandCells = new List<GridCell>();

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
        Debug.Log(hexGrid.LandHexes.Count + " land hexes to copy");
        foreach (FlatHexPoint point in hexGrid.LandHexes)
        {
            LandCells.Add(new GridCell(point.Y, point.X));
        }

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

        // Write envConfig file (keep this commented out normally)
        //WriteEnvConfigFile();

        // Check to see if we have exactly 1 blue base
        if (BlueBases.Count != 1)
        {
            soaEventLogger.LogError("Fatal Error: Blue base count must be 1, instead found " + BlueBases.Count + " at initialization");
            TerminateSimulation();
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
            else if (platform.tag.Contains("BlueBalloon"))
            {
                ActivateBlueBalloon(platform);
            }
            else
            {
                Debug.LogWarning("Error activating remote platform, unrecognized tag " + platform.tag);
            }
        }

        // Last thing to do is to start comms with all beliefs in data
        // manager already initialized
        redDataManager.startComms();
        blueDataManager.startComms();
	} // End Start()

    // Reads in the XML config file
    void LoadConfigFile()
    {
        // Parse the XML config file
        soaConfig = SoaConfigXMLReader.Parse(ConfigFileName);

        // Logger settings
        soaEventLogger = new SoaEventLogger(soaConfig.loggerOutputFile,
            ConfigFileName, soaConfig.enableLogToFile, soaConfig.enableLogEventsToFile, 
            soaConfig.enableLogToUnityConsole);

        // Game duration
        gameDurationHr = soaConfig.gameDurationHr;

        // Red platform weapon probability
        probRedTruckHasWeapon = soaConfig.probRedTruckHasWeapon;
        probRedDismountHasWeapon = soaConfig.probRedDismountHasWeapon;
        probRedTruckHasJammer = soaConfig.probRedTruckHasJammer;

        // Comms and jammer range defaults
        defaultCommsRanges = soaConfig.defaultCommsRanges;
        defaultJammerRanges = soaConfig.defaultJammerRanges;

        // Network settings
        networkRedRoom = soaConfig.networkRedRoom;
        networkBlueRoom = soaConfig.networkBlueRoom;

        // Set up sites
        foreach (SiteConfig s in soaConfig.sites)
        {
            switch (s.GetConfigType())
            {
                case SiteConfig.ConfigType.BLUE_BASE:
                    InstantiateBlueBase((BlueBaseConfig)s);
                    break;
                case SiteConfig.ConfigType.RED_BASE:
                    InstantiateRedBase((RedBaseConfig)s);
                    break;
                case SiteConfig.ConfigType.NGO_SITE:
                    InstantiateNGOSite((NGOSiteConfig)s);
                    break;
                case SiteConfig.ConfigType.VILLAGE:
                    InstantiateVillage((VillageConfig)s);
                    break;
            }
        }

        // Set up remote platforms
        foreach (PlatformConfig p in soaConfig.remotePlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.HEAVY_UAV:
                    InstantiateHeavyUAV((HeavyUAVConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.SMALL_UAV:
                    InstantiateSmallUAV((SmallUAVConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.BLUE_BALLOON:
                    InstantiateBlueBalloon((BlueBalloonConfig)p, false);
                    break;
            }
        }

        // Set up local platforms
        foreach (PlatformConfig p in soaConfig.localPlatforms)
        {
            switch (p.GetConfigType())
            {
                case PlatformConfig.ConfigType.RED_DISMOUNT:
                    InstantiateRedDismount((RedDismountConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.RED_TRUCK:
                    InstantiateRedTruck((RedTruckConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.NEUTRAL_DISMOUNT:
                    InstantiateNeutralDismount((NeutralDismountConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.NEUTRAL_TRUCK:
                    InstantiateNeutralTruck((NeutralTruckConfig)p, false);
                    break;
                case PlatformConfig.ConfigType.BLUE_POLICE:
                    InstantiateBluePolice((BluePoliceConfig)p, false);
                    break;
            }
        }
    }

    // Sends out initial map beliefs through the blue data manager
    void PushInitialMapBeliefs()
    {
        GameObject g;
        FlatHexPoint currentCell;

        b = new Belief_GridSpec(64, 36, gridOrigin_x, gridOrigin_z, gridToWorldScale);
        blueDataManager.addBeliefToAllActors(b, 0);
        blueDataManager.addInitializationBelief(b);
        //Debug.Log(b.ToString());

        b = new Belief_Terrain((int)soa.Terrain.MOUNTAIN, MountainCells);
        blueDataManager.addBeliefToAllActors(b, 0);
        blueDataManager.addInitializationBelief(b);
        //Debug.Log(MountainCells.Count + " mountain hexes.");

        b = new Belief_Terrain((int)soa.Terrain.WATER, WaterCells);
        blueDataManager.addBeliefToAllActors(b, 0);
        blueDataManager.addInitializationBelief(b);
        //Debug.Log(WaterCells.Count + " water hexes.");

        for (int i = 0; i < BlueBases.Count; i++)
        {
            g = BlueBases[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            BlueBaseSim s = g.GetComponent<BlueBaseSim>();
            b = new Belief_Base(i, theseCells, s.Supply);
            blueDataManager.addBeliefToAllActors(b, 0);
            blueDataManager.addInitializationBelief(b);
            //Debug.Log(b.ToString());
        }

        // Unique ID for ngo/villages
        int destination_id = 0;

        for (int i = 0; i < NgoSites.Count; i++)
        {
            g = NgoSites[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            NgoSim s = g.GetComponent<NgoSim>();
            s.destination_id = destination_id;
            b = new Belief_NGOSite(destination_id, theseCells, s.Supply, s.Casualties, s.Civilians);
            blueDataManager.addBeliefToAllActors(b, 0);
            blueDataManager.addInitializationBelief(b);
            //Debug.Log(b.ToString());

            // Increment destination id (unique across all ngo and villages)
            destination_id++;
        }

        for (int i = 0; i < Villages.Count; i++)
        {
            g = Villages[i];
            List<GridCell> theseCells = new List<GridCell>();
            currentCell = hexGrid.Map[g.transform.position];
            theseCells.Add(new GridCell(currentCell.Y, currentCell.X));
            VillageSim s = g.GetComponent<VillageSim>();
            s.destination_id = destination_id;
            b = new Belief_Village(destination_id, theseCells, s.Supply, s.Casualties);
            blueDataManager.addBeliefToAllActors(b, 0);
            blueDataManager.addInitializationBelief(b);
            //Debug.Log(b.ToString());

            // Increment destination id (unique across all ngo and villages)
            destination_id++;
        }
    }

    void WriteEnvConfigFile()
    {
        // Make a new config file
        EnvConfig envConfig = new EnvConfig();

        // Populate gridspec
        envConfig.gridOrigin_x = gridOrigin_x;
        envConfig.gridOrigin_z = gridOrigin_z;
        envConfig.gridToWorldScale = gridToWorldScale;

        // Terrain information
        foreach (GridCell g in MountainCells)
        {
            envConfig.mountainCells.Add(new PrimitivePair<int, int>(g.getRow(), g.getCol()));
        }
        foreach (GridCell g in WaterCells)
        {
            envConfig.waterCells.Add(new PrimitivePair<int, int>(g.getRow(), g.getCol()));
        }
        foreach (GridCell g in LandCells)
        {
            envConfig.landCells.Add(new PrimitivePair<int, int>(g.getRow(), g.getCol()));
        }

        // No road information for now

        // Site information
        FlatHexPoint currentCell;
        for (int i = 0; i < BlueBases.Count; i++)
        {
            currentCell = hexGrid.Map[BlueBases[i].transform.position];
            envConfig.blueBaseCells.Add(new PrimitivePair<int, int>(currentCell.Y, currentCell.X));
        }
        for (int i = 0; i < RedBases.Count; i++)
        {
            currentCell = hexGrid.Map[RedBases[i].transform.position];
            envConfig.redBaseCells.Add(new PrimitivePair<int, int>(currentCell.Y, currentCell.X));
        }
        for (int i = 0; i < NgoSites.Count; i++)
        {
            currentCell = hexGrid.Map[NgoSites[i].transform.position];
            envConfig.ngoSiteCells.Add(new PrimitivePair<int, int>(currentCell.Y, currentCell.X));
        }
        for (int i = 0; i < Villages.Count; i++)
        {
            currentCell = hexGrid.Map[Villages[i].transform.position];
            envConfig.villageCells.Add(new PrimitivePair<int, int>(currentCell.Y, currentCell.X));
        }

        // Write to file
        EnvConfigXMLWriter.Write(envConfig, EnvFileName);
    }
    #endregion

    #region Update
    /*****************************************************************************************************
     *                                              UPDATE                                               *
     *****************************************************************************************************/
    // Update is called once per frame
    private bool firstUpdate = true;
	void Update () 
    {
        if (firstUpdate)
        {
            // Add map beliefs to outgoing queue
            PushInitialMapBeliefs();
            firstUpdate = false;
        }

        // Update mouse over functions
        UpdateMouseOver();

        // Record the time in seconds it took to complete the last frame
        float dt = Time.deltaTime;

        // Update game clock
        gameClockHr += (dt / 60); // Note: 1 min real time = 1 hr simulated time

        // Update sensor clock
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

        // Update timer
        updateTimer += dt;
        if (updateTimer > updateRateS)
        {
            bool terminationConditionsMet = false;
            // Check for game termination conditions
            if (gameClockHr >= gameDurationHr)
            {
                // Condition 1: Game timer maxed out
                terminationConditionsMet = true;
            }
            else
            {
                // Condition 2: If all remote controlled heavy and small UAVs are dead
                terminationConditionsMet = true;
                foreach (GameObject g in RemotePlatforms)
                {
                    // Conditions not met if there is a small UAV or heavy UAV that is alive
                    // Note: Balloons don't count
                    if ((g.tag.Contains("SmallUAV") || g.tag.Contains("HeavyUAV")) && 
                        g.GetComponent<SoaActor>().isAlive)
                    {
                        terminationConditionsMet = false;
                        break;
                    }
                }
            }

            // Quit the game if termination conditions are met
            if (terminationConditionsMet)
            {
                TerminateSimulation();
                return;
            }

            // Compute distances between blue/red units for comms purposes
            blueDataManager.calculateDistances();
            redDataManager.calculateDistances();
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


            if (BroadcastOn)
            {
                int count = 1;
                while (count < 2)
                {
                    count++;
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
                                if (actor.affiliation != Affiliation.NEUTRAL)
                                {
                                    actor.broadcastCommsLocal();
                                }
                            }

                            for (int i = 0; i < RemotePlatforms.Count; i++)
                            {
                                GameObject platform = RemotePlatforms[i];
                                SoaActor actor = platform.GetComponent<SoaActor>();
                                actor.broadcastCommsLocal();
                            }

                            for (int i = 0; i < BlueBases.Count; i++)
                            {
                                GameObject platform = BlueBases[i];
                                SoaSite site = platform.GetComponent<SoaSite>();
                                site.broadcastCommsLocal();
                            }


                            //Merge in belief dictionary after round of comms
                            for (int i = 0; i < LocalPlatforms.Count; i++)
                            {
                                GameObject platform = LocalPlatforms[i];
                                SoaActor actor = platform.GetComponent<SoaActor>();
                                if (actor.affiliation != Affiliation.NEUTRAL)
                                {
                                    actor.mergeBeliefDictionary();
                                }
                                
                            }

                            for (int i = 0; i < RemotePlatforms.Count; i++)
                            {
                                GameObject platform = RemotePlatforms[i];
                                SoaActor actor = platform.GetComponent<SoaActor>();
                                actor.mergeBeliefDictionary();
                            }

                            for (int i = 0; i < BlueBases.Count; i++)
                            {
                                GameObject platform = BlueBases[i];
                                SoaSite site = platform.GetComponent<SoaSite>();
                                site.mergeBeliefDictionary();
                            }
                        }
                    }
                }

                for (int i = 0; i < RemotePlatforms.Count; i++)
                {
                    GameObject platform = RemotePlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();
                    actor.updateRemoteAgent();
                }

                for (int i = 0; i < LocalPlatforms.Count; i++)
                {
                    GameObject platform = LocalPlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();
                    if (!actor.isAlive)
                    {
                        if (actor.affiliation == Affiliation.BLUE)
                        {
                            DestroyLocalBluePlatform(platform);
                        }
                    }
                }

                for (int i = 0; i < RemotePlatforms.Count; i++)
                {
                    GameObject platform = RemotePlatforms[i];
                    SoaActor actor = platform.GetComponent<SoaActor>();
                    if (!actor.isAlive)
                    {
                        DestroyRemoteBluePlatform(platform);
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
    private void TerminateSimulation()
    {
        Application.Quit(); // For when running as standalone
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For when running in editor 
        #endif
    }
    
    void OnApplicationQuit()
    {
        // Stop data managers / comms managers
        redDataManager.stopPhoton();
        blueDataManager.stopPhoton();

        // Stop logger and write output to file
        soaEventLogger.TerminateLogging();
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
            s.addBeliefToBeliefDictionary(b);
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

    private void SetPerceptionCapabilities(GameObject g, PlatformConfig config, string platformName)
    {
        // Set perception capabilities
        SoaSensor soaSensor = g.GetComponentInChildren<SoaSensor>();
        if (soaSensor != null)
        {
            soaSensor.beamwidthDeg = config.GetUseDefaultSensorBeamwidth() ? soaConfig.defaultSensorBeamwidths[platformName] : config.GetSensorBeamwidth();
            soaSensor.modes = config.GetUseDefaultSensorModalities() ? soaConfig.defaultSensorModalities[platformName].ToArray() : config.GetSensorModalities().ToArray();
        }
        SoaClassifier soaClassifier = g.GetComponentInChildren<SoaClassifier>();
        if (soaClassifier != null)
        {
            soaClassifier.modes = config.GetUseDefaultClassifierModalities() ? soaConfig.defaultClassifierModalities[platformName].ToArray() : config.GetClassifierModalities().ToArray();
        }
    }

    private bool CheckInitialLocation(Vector3 worldPosition, bool landEnabled, bool waterEnabled, bool mountainEnabled, out Vector3 snappedPosition)
    {
        // Convert world coordinates to grid coordinates
        PrimitivePair<float, float> worldPos = new PrimitivePair<float,float> (worldPosition.x, worldPosition.z);
        PrimitivePair<int, int> gridPos = gridMath.WorldToGrid(worldPos);
        GridCell initialCell = new GridCell(gridPos.second, gridPos.first);

        // Snap world position to a grid centroid
        PrimitivePair<float, float> snappedPos = gridMath.GridToWorld(gridPos);
        snappedPosition = new Vector3(snappedPos.x, 0, snappedPos.z);

        // Check against cell types
        bool found = false;

        if (landEnabled)
        {
            foreach (GridCell g in LandCells)
            {
                found = found || g.Equals(initialCell);
            }
        }
        if (waterEnabled)
        {
            foreach (GridCell g in WaterCells)
            {
                found = found || g.Equals(initialCell);
            }
        }
        if (mountainEnabled)
        {
            foreach (GridCell g in MountainCells)
            {
                found = found || g.Equals(initialCell);
            }
        }

        if (!found)
        {
            Debug.LogError("SimControl::CheckInitialLocation(): Cannot place unit at desired location");
        }

        return found;
    }

    #endregion

    #region Site Instantiation
    /*****************************************************************************************************
     *                                        SITE INSTANTIATION                                         *
     *****************************************************************************************************/
    public GameObject InstantiateBlueBase(BlueBaseConfig c)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);

        // Blue base must exist on land
        Vector3 snappedPos;
        if (CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate at the snapped position
            GameObject g = (GameObject)Instantiate(BlueBasePrefab, newPos * KmToUnity, Quaternion.identity);

            // Blue bases get id 0 always
            SoaSite s = g.GetComponent<SoaSite>();
            s.unique_id = 0;
            g.name = (c.name == null) ? "Blue Base" : c.name;

            // Set comms capabilties
            s.commsRange_km = c.commsRange_km;

            // Add to appropriate lists
            LocalPlatforms.Add(g);
            BlueBases.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Blue base not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateRedBase(RedBaseConfig c)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);

        // Red base must exist on land
        Vector3 snappedPos;
        if (CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate at the snapped position
            GameObject g = (GameObject)Instantiate(RedBasePrefab, newPos * KmToUnity, Quaternion.identity);

            // Assign name
            g.name = (c.name == null) ? ("Red Base " + RedBases.Count) : c.name;

            // Add to appropriate lists
            RedBases.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Red base not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateNGOSite(NGOSiteConfig c)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);

        // NGO site must exist on land
        Vector3 snappedPos;
        if (CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate at the snapped position
            GameObject g = (GameObject)Instantiate(NGOSitePrefab, newPos * KmToUnity, Quaternion.identity);

            // Assign name
            g.name = (c.name == null) ? ("NGO Site " + NgoSites.Count) : c.name;

            // Add to appropriate lists
            NgoSites.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: NGO Site not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateVillage(VillageConfig c)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);

        // NGO site must exist on land
        Vector3 snappedPos;
        if (CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate at the snapped position
            GameObject g = (GameObject)Instantiate(VillagePrefab, newPos * KmToUnity, Quaternion.identity);

            // Assign name
            g.name = (c.name == null) ? ("Village " + Villages.Count) : c.name;

            // Add to appropriate lists
            Villages.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Village not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }
    #endregion

    #region Remote Platform Instantiation
    /*****************************************************************************************************
     *                                  REMOTE PLATFORM INSTANTIATION                                    *
     *****************************************************************************************************/
    public GameObject InstantiateHeavyUAV(HeavyUAVConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, c.y_km, c.z_km);

        // Heavy UAV can only traverse on land and water
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, true, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(HeavyUAVPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Heavy UAV " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(c.y_km);

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "HeavyUAV");

            // Set comms capabilities
            a.commsRange_km = c.commsRange_km;
            
            // Set fuel tank size
            HeavyUAVSim h = g.GetComponent<HeavyUAVSim>();
            h.fuelTankSize_s = c.fuelTankSize_s;

            // Add to list of remote platforms
            RemotePlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Heavy UAV not instantiated since initial position " + newPos + " not on land or water cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateSmallUAV(SmallUAVConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, c.y_km, c.z_km);

        // Small UAV can only traverse on land and water
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, true, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(SmallUAVPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Small UAV " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(c.y_km);

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "SmallUAV");

            // Set comms capabilities
            a.commsRange_km = c.commsRange_km;

            // Set fuel tank size
            SmallUAVSim s = g.GetComponent<SmallUAVSim>();
            s.fuelTankSize_s = c.fuelTankSize_s;

            // Add to list of remote platforms
            RemotePlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Small UAV not instantiated since initial position " + newPos + " not on land or water");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateBlueBalloon(BlueBalloonConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 15, c.z_km);
        
        // Balloon can traverse on land, water, and mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, true, true, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(BlueBalloonPrefab, newPos * KmToUnity, Quaternion.identity);
                       
            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Blue Balloon " + a.unique_id;
                      
            // Assign initial altitude
            a.SetSimAltitude(15);
        
            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "BlueBalloon");
         
            // Add to list of remote platforms
            RemotePlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Blue balloon not instantiated since initial position " + newPos + " not on land, water, or mountain cell");
            TerminateSimulation();
            return null;
        }
    }
    #endregion

    #region Remote Platform Activation
    /*****************************************************************************************************
     *                                    REMOTE PLATFORM ACTIVATION                                     *
     *****************************************************************************************************/
    private void ActivateRemotePlatform(GameObject platform, bool useExternalWaypoint)
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
        actor.useExternalWaypoint = useExternalWaypoint;

        // Data manager
        actor.dataManager = blueDataManager;
        blueDataManager.addNewActor(actor);
    }

    public void ActivateHeavyUAV(GameObject platform)
    {
        ActivateRemotePlatform(platform, true);
    }

    public void ActivateSmallUAV(GameObject platform)
    {
        ActivateRemotePlatform(platform, true);
    }

    public void ActivateBlueBalloon(GameObject platform)
    {
        ActivateRemotePlatform(platform, false);
    }
    #endregion

    #region Local Platform Instantiation
    /*****************************************************************************************************
     *                                   LOCAL PLATFORM INSTANTIATION                                    *
     *****************************************************************************************************/
    public GameObject InstantiateRedDismount(RedDismountConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);
        
        // Red dismount can only traverse on land, not water or mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(RedDismountPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Red Dismount " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(0); // Red dismounts stay on the ground

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

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "RedDismount");

            // Set comms capabilities
            a.commsRange_km = c.commsRange_km;

            // Add to list of local platforms
            LocalPlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Red dismount not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateRedTruck(RedTruckConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);
        //Debug.Log("Initial pos " + newPos.ToString());

        // Red truck can only traverse on land, not water or mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            Debug.Log("Instantiate red truck");
            // Instantiate
            GameObject g = (GameObject)Instantiate(RedTruckPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Red Truck " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(0); // Red trucks stay on the ground

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

            // Set comms capabilities
            a.commsRange_km = c.commsRange_km;

            // Set jammer capabilties
            SoaJammer jammer = g.GetComponentInChildren<SoaJammer>();
            jammer.effectiveRange_km = c.jammerRange_km;
            jammer.isOn = c.hasJammer;
            jammers.Add(jammer);
            Debug.Log("Has jammer " + jammer.isOn + ", effective range " + jammer.effectiveRange_km);
            Debug.Log("Jammer list size: " + jammers.Count);

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "RedTruck");

            // Add to list of local platforms
            LocalPlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Red truck not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateNeutralDismount(NeutralDismountConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);
        
        // Neutral dismount can only traverse on land, not water or mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(NeutralDismountPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Neutral Dismount " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(0); // Neutral dismounts stay on the ground

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "NeutralDismount");

            // Add to list of local platforms
            LocalPlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Neutral dismount not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateNeutralTruck(NeutralTruckConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);
        
        // Neutral truck can only traverse on land, not water or mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(NeutralTruckPrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Neutral Truck " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(0); // Neutral trucks stay on the ground

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "NeutralTruck");

            // Add to list of local platforms
            LocalPlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Neutral truck not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
    }

    public GameObject InstantiateBluePolice(BluePoliceConfig c, bool initialLocationCheckOverride)
    {
        // Proposed initial position
        Vector3 newPos = new Vector3(c.x_km, 0, c.z_km);
        
        // Blue police can only traverse on land, not water or mountains
        Vector3 snappedPos;
        if (initialLocationCheckOverride || CheckInitialLocation(newPos, true, false, false, out snappedPos))
        {
            // Instantiate
            GameObject g = (GameObject)Instantiate(BluePolicePrefab, newPos * KmToUnity, Quaternion.identity);

            // Set grid
            g.GetComponent<TrackOnGrid>().hexGrid = hexGrid;

            // Assign unique ID
            SoaActor a = g.GetComponent<SoaActor>();
            a.unique_id = RequestUniqueID(c.id);
            g.name = "Blue Police " + a.unique_id;

            // Assign initial altitude
            a.SetSimAltitude(0); // Blue police stay on the ground

            // Set perception capabilities
            SetPerceptionCapabilities(g, c, "BluePolice");

            // Set comms capabilities
            a.commsRange_km = c.commsRange_km;

            // Add to list of local platforms
            LocalPlatforms.Add(g);
            return g;
        }
        else
        {
            soaEventLogger.LogError("Fatal Error: Blue police not instantiated since initial position " + newPos + " not on land cell");
            TerminateSimulation();
            return null;
        }
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

    private void DestroyLocalBluePlatform(GameObject platform)
    {
        // Remove from mouse script
        omcScript.DeletePlatform(platform);

        // Remove from data manager
        blueDataManager.removeActor(platform.GetComponent<SoaActor>());

        // Remove from local platform list
        LocalPlatforms.Remove(platform);

        // Destroy now
        Destroy(platform);
    }

    private void DestroyRemoteBluePlatform(GameObject platform)
    {
        // Remove from mouse script
        omcScript.DeletePlatform(platform);

        Debug.LogWarning("DESTROYING " + platform.name);

        // Remove from data manager
        blueDataManager.removeActor(platform.GetComponent<SoaActor>());

        // Remove from local platform list
        RemotePlatforms.Remove(platform);

        // Destroy now
        Destroy(platform);
    }

    public void DestroyRedDismount(GameObject platform)
    {
        DestroyLocalRedPlatform(platform);
    }

    public void DestroyRedTruck(GameObject platform)
    {
        // Remove jammer from global list
        jammers.Remove(platform.GetComponentInChildren<SoaJammer>());

        // Standard destroy procedure
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
