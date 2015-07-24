using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using soa;
using Gamelogic.Grids;

public enum Affiliation { BLUE = 0, RED = 1, NEUTRAL = 2 };
public class SimControl : MonoBehaviour 
{
    static public float KmToUnity;

    public string RedRoom = "soa-apl-red";
    public string BlueRoom = "soa-apl-blue";
    public List<GameObject> LocalPlatforms;
    public List<GameObject> RemotePlatforms;
    public List<GameObject> NgoSites;
    public List<GameObject> Villages;
    public List<GameObject> RedBases;
    public List<GameObject> BlueBases;
    public List<GridCell> MountainCells;
    public List<GridCell> WaterCells;
    public bool BroadcastOn;
    
    public OverheadMouseCamera omcScript;
    public SoaHexWorld hexGrid;

    public float updateRateS;
    private bool showTruePositions = true;

    //only access this when you have the DataManager lock
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayBeliefDictionary;
	// Use this for initialization

    DataManager redDataManager;
    DataManager blueDataManager;

    public Canvas uiCanvas;
    public GameObject labelUI;
    GameObject labelInstance;
    RectTransform labelTransform;
    Vector3 labelPosition;
    Text[] labels;
    Camera thisCamera;

	void Start () 
    {
        KmToUnity = hexGrid.KmToUnity();
        Debug.Log("Km to Unity = " + KmToUnity);

        WaterCells = new List<GridCell>();
        MountainCells = new List<GridCell>();

        Debug.Log(hexGrid.WaterHexes.Count + " water hexes to copy");
        foreach (FlatHexPoint point in hexGrid.WaterHexes)
            WaterCells.Add(new GridCell(point.Y, point.X));

        Debug.Log(hexGrid.MountainHexes.Count + " mountain hexes to copy");
        foreach (FlatHexPoint point in hexGrid.MountainHexes)
            MountainCells.Add(new GridCell(point.Y, point.X));

        LoadConfigFile();

        thisCamera = omcScript.GetComponent<Camera>();

        NavMesh.pathfindingIterationsPerFrame = 50;

        //if (BroadcastOn)
        //{
            // Create manager
            redDataManager = new DataManager(RedRoom);
            blueDataManager = new DataManager(BlueRoom);
            displayBeliefDictionary = blueDataManager.getGodsEyeView();

        //}

        for (int i=0; i<LocalPlatforms.Count; i++)
        {
            GameObject platform = LocalPlatforms[i];
            Debug.Log("Adding platform " + platform.name);
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            actor.unique_id = 200 + i;
            //actor.simulateMotion = true;

            if (platform.name.Contains("Blue Base"))
            {
                actor.unique_id = 0;
            }


            if (platform.name.Contains("Blue"))
            {
                //actor.affiliation = (int)Affiliation.BLUE;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("HeavyLift"))
            {
                //actor.affiliation = 0;
                //actor.type = 2;
                //actor.commsRange = 5000;
                //actor.useExternalWaypoint = true;
            }
            if (platform.name.Contains("SmallUav"))
            {
                //actor.affiliation = 0;
                //actor.type = 3;
                //actor.commsRange = 5000;
                //actor.useExternalWaypoint = true;
            }
            if(platform.name.Contains("Red"))
            {
                //actor.affiliation = 1;
                actor.useExternalWaypoint = false;
                actor.dataManager = redDataManager;
                redDataManager.addNewActor(actor);
            }

        }

        Debug.Log("Adding remote platforms");
        for (int i = 0; i < RemotePlatforms.Count; i++)
        {
            GameObject platform = RemotePlatforms[i];
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            actor.unique_id = 100 + i;

            Debug.Log("Adding platform " + platform.name + " id " + actor.unique_id);
            actor.simulateMotion = true;

            if (platform.name.Contains("Blue"))
            {
                //actor.affiliation = (int)Affiliation.BLUE;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("HeavyLift"))
            {
                //actor.affiliation = (int)Affiliation.BLUE;
                //actor.type = 2;
                //actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("SmallUav"))
            {
                //actor.affiliation = 0;
                //actor.type = 3;
                //actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("Red"))
            {
                //actor.affiliation = 1;
                actor.dataManager = redDataManager;
                actor.useExternalWaypoint = false;
                redDataManager.addNewActor(actor);
            }

            if (platform.name.Contains("Truck"))
            {
                //actor.type = 0;
                //actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Dismount"))
            {
                //actor.type = 1;
                //actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Police"))
            {
                //actor.type = 4;
                //actor.commsRange = 5000;
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

    string ConfigFileName = "SoaSimConfig.xml";
    void LoadConfigFile()
    {
        Debug.Log("Loading from " + ConfigFileName);

        XmlTextReader reader = new XmlTextReader(ConfigFileName);

        reader.ReadToDescendant("RedRoom");
        Debug.Log(reader.Name);
        RedRoom = reader.ReadElementContentAsString();
        Debug.Log(RedRoom);
        reader.ReadToFollowing("BlueRoom");
        Debug.Log(reader.Name);
        BlueRoom = reader.ReadElementContentAsString();
        Debug.Log(BlueRoom);

        reader.Close();
    }

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
}
