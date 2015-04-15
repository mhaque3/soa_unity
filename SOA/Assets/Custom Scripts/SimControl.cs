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

	// Use this for initialization

    PhotonCloudCommManager cm;
	void Start () 
    {
        NavMesh.pathfindingIterationsPerFrame = 50;

        if (BroadcastOn)
        {
            // Create managers
            DataManager dm = new DataManager();
            Serializer ps = new ProtobufSerializer();

            cm = new PhotonCloudCommManager(dm, ps, "app-us.exitgamescloud.com:5055", "soa");
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

            // Start them
            cm.start();
        }

        for (int i=0; i<LocalPlatforms.Count; i++)
        {
            GameObject platform = LocalPlatforms[i];
            Debug.Log("Adding platform " + platform.name);
            omcScript.AddPlatform(platform);

            SoaActor actor = platform.GetComponent<SoaActor>();
            actor.unique_id = i;

            if (platform.name.Contains("Blue"))
            {
                actor.affiliation = 0;
            }
            if (platform.name.Contains("HeavyLift"))
            {
                actor.affiliation = 0;
                actor.type = 2;
            }
            if (platform.name.Contains("SmallUAV"))
            {
                actor.affiliation = 0;
                actor.type = 3;
            }
            if(platform.name.Contains("Red"))
            {
                actor.affiliation = 1;
            }

            if (platform.name.Contains("Truck"))
            {
                actor.type = 0;
            }
            if (platform.name.Contains("Dismount"))
            {
                actor.type = 1;
            }
            if (platform.name.Contains("Police"))
            {
                actor.type = 4;
            }
            
        }
	}
	
	// Update is called once per frame

    int randDraw;
    Belief b;
    float messageTimer = 0f;
	void Update () 
    {
        if (BroadcastOn)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer > 5f)
            {

                for (int i = 0; i < LocalPlatforms.Count; i++)
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
                Debug.Log("*** END OUTGOING MESSAGE BLOCK ***");
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
        if(cm != null)
            cm.terminate();
    }
}
