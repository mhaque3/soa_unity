using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using soa;

public class SimControl : MonoBehaviour 
{

    public List<GameObject> LocalPlatforms;
    public List<GameObject> RemotePlatforms;
    public List<GameObject> NgoSites;
    public List<GameObject> Villages;
    public List<GameObject> RedBases;
    public bool BroadcastOn;
    public OverheadMouseCamera omcScript;
    public float updateRateS;
    private bool showTruePositions = true;

    //only access this when you have the DataManager lock
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayBeliefDictionary;
	// Use this for initialization

    DataManager redDataManager;
    DataManager blueDataManager;

    enum Affiliation { BLUE = 0, RED = 1, NEUTURAL = 2 };

	void Start () 
    {
        NavMesh.pathfindingIterationsPerFrame = 50;

        //if (BroadcastOn)
        //{
            // Create manager
            redDataManager = new DataManager("soa-apl-red");
            blueDataManager = new DataManager("soa-apl-blue");
            displayBeliefDictionary = blueDataManager.getGodsEyeView();

        //}

        for (int i=0; i<LocalPlatforms.Count; i++)
        {
            GameObject platform = LocalPlatforms[i];
            Debug.Log("Adding platform " + platform.name);
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            actor.unique_id = 200 + i;
            actor.simulateMotion = true;

            if (platform.name.Contains("Blue"))
            {
                actor.affiliation = (int)Affiliation.BLUE;
                actor.dataManager = blueDataManager;
            }
            if (platform.name.Contains("HeavyLift"))
            {
                actor.affiliation = 0;
                actor.type = 2;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
            }
            if (platform.name.Contains("SmallUav"))
            {
                actor.affiliation = 0;
                actor.type = 3;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
            }
            if(platform.name.Contains("Red"))
            {
                actor.affiliation = 1;
                actor.useExternalWaypoint = false;
                actor.dataManager = redDataManager;
                redDataManager.addNewActor(actor);
            }

            if (platform.name.Contains("Truck"))
            {
                actor.type = 0;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Dismount"))
            {
                actor.type = 1;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Police"))
            {
                actor.unique_id = 200-i;
                actor.type = 4;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
                blueDataManager.addNewActor(actor);
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
                actor.affiliation = (int)Affiliation.BLUE;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("HeavyLift"))
            {
                actor.affiliation = (int)Affiliation.BLUE;
                actor.type = 2;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("SmallUav"))
            {
                actor.affiliation = 0;
                actor.type = 3;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = true;
                actor.dataManager = blueDataManager;
                blueDataManager.addNewActor(actor);
            }
            if (platform.name.Contains("Red"))
            {
                actor.affiliation = 1;
                actor.dataManager = redDataManager;
                actor.useExternalWaypoint = false;
                redDataManager.addNewActor(actor);
            }

            if (platform.name.Contains("Truck"))
            {
                actor.type = 0;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Dismount"))
            {
                actor.type = 1;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }
            if (platform.name.Contains("Police"))
            {
                actor.type = 4;
                actor.commsRange = 5000;
                actor.useExternalWaypoint = false;
            }

        }
	}
	
	// Update is called once per frame

    int randDraw;
    Belief b;
    float messageTimer = 0f;
    float updateTimer = 0f;
	void Update () 
    {
        updateTimer += Time.deltaTime;
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
                    if (actor.affiliation == (int)Affiliation.RED)
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
                    if (actor.affiliation == (int)Affiliation.BLUE)
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

                updateTimer = 0f;
            }
        }

        if (BroadcastOn)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer > updateRateS / 2f)
            {
                //This lock keeps the comms manager from adding data while we pushing out comms
                lock (redDataManager.dataManagerLock)
                {
                    

                    //Get the current belief map to display.  Default is the data managers map which is the gods eye view.
                    SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> displayMap = redDataManager.getGodsEyeView();

                    //TODO Update local platforms comms
                    for (int i = 0; i < LocalPlatforms.Count; i++)
                    {
                        GameObject platform = LocalPlatforms[i];
                        SoaActor actor = platform.GetComponent<SoaActor>();
                        actor.broadcastComms();
                    }
                    
                    /*for (int i = 0; i < LocalPlatforms.Count; i++)
                    {
                        GameObject platform = LocalPlatforms[i];
                        SoldierWaypointMotion motion = platform.GetComponent<SoldierWaypointMotion>();
                        Vector3 velocity = platform.transform.forward * motion.GetSpeed();
                        SoaActor actor = platform.GetComponent<SoaActor>();
                        b = new Belief_Actor(actor.unique_id, actor.affiliation, actor.type,
                                platform.transform.position.x, platform.transform.position.y, platform.transform.position.z,
                                true, velocity.x, true, velocity.y, true, velocity.z);
                        Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType() + "\n" + b);
                        cm.addOutgoing(b);
                    }
                    */
                    messageTimer = 0f;

                    /*
                    // Create 64 bit time field
                    ulong randTime = (ulong)Random.Range(ulong.MinValue, ulong.MaxValue);

                    // Create and send beliefs		
                    randDraw = Random.Range(0, 10);
                    switch (randDraw)
                    {
                        case 0:
                            b = new Belief_Actor((int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)),
                                (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), true, (int)(Random.Range(int.MinValue, int.MaxValue)),
                                false, (int)(Random.Range(int.MinValue, int.MaxValue)), true, (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 1:
                            b = new Belief_BaseCell((int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 2:
                            b = new Belief_Mode_Command(randTime, (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 3:
                            b = new Belief_NGOSiteCell((int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 4:
                            b = new Belief_Nogo((int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 5:
                            b = new Belief_Road(true, (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 6:
                            b = new Belief_SPOI(randTime, (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 7:
                            b = new Belief_Time(randTime);
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 8:
                            b = new Belief_VillageCell((int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 9:
                            b = new Belief_Waypoint(randTime, (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                        case 10:
                            b = new Belief_Waypoint_Override(randTime, (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)), (int)(Random.Range(int.MinValue, int.MaxValue)));
                            Debug.Log("Test: Enqueueing belief type " + (int)b.getBeliefType());
                            cm.addOutgoing(b);
                            break;
                    }
                     * */
                }
                //Debug.Log("*** END OUTGOING MESSAGE BLOCK ***");
            }
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
