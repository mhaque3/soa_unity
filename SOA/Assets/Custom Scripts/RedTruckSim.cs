using UnityEngine;
using System.Collections;

public class RedTruckSim : MonoBehaviour 
{
    SimControl simControlScript;
    SoldierWaypointMotion waypointScript;
    NavMeshAgent thisNavAgent;
    public bool Civilian;
    public GameObject CivilianIcon;

	// Use this for initialization
	void Start () 
    {
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        waypointScript = gameObject.GetComponent<SoldierWaypointMotion>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
    public float PathUpdateInterval;
    float PathUpdateClock = 0f;
	void Update () 
    {
        float dt = Time.deltaTime;

        CivilianIcon.SetActive(Civilian);

        PathUpdateClock += dt;
        if (PathUpdateClock > PathUpdateInterval)
        {
            thisNavAgent.ResetPath();
            PathUpdateClock = Random.value * PathUpdateInterval * 0.5f;
            //thisNavAgent.SetDestination(waypointScript.targetPosition);
        }
	}

    float PathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return 0f;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        while (i < path.corners.Length)
        {
            Vector3 currentCorner = path.corners[i];
            lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
            i++;
        }
        return lengthSoFar;
    }

    GameObject GetRetreatRedBase(BluePoliceSim threat)
    {
        GameObject selectedRedBase = simControlScript.RedBases[0];
        float minRangeRatio = float.MaxValue;
        foreach (GameObject redBase in simControlScript.RedBases)
        {
            NavMeshPath thisPath = new NavMeshPath();
            if(thisNavAgent.CalculatePath(redBase.transform.position, thisPath))
            {
                float thisLength = PathLength(thisPath);
                float threatRange = threat.GetRangeTo(redBase);
                if (threatRange > 0f)
                {
                    float thisRatio = thisLength/threatRange;
                    if (thisRatio < minRangeRatio)
                    {
                        minRangeRatio = thisRatio;
                        selectedRedBase = redBase;
                    }
                }
            }
        }
        return selectedRedBase;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BluePolice"))
        {
            BluePoliceSim b = other.gameObject.GetComponent<BluePoliceSim>();
            if (b != null)
            {
                waypointScript.On = false;
                thisNavAgent.ResetPath();
                waypointScript.waypointIndex = 0;
                waypointScript.waypoints.Clear();
                waypointScript.waypoints.Add(GetRetreatRedBase(b));
                waypointScript.On = true;

                if (Civilian)
                {
                    Civilian = false;
                }
            }
        }

        if (other.CompareTag("RedBase"))
        {
            RedBaseSim rb = other.gameObject.GetComponent<RedBaseSim>();
            if (rb != null)
            {
                if (Civilian)
                {
                    Civilian = false;
                    rb.Civilians++;
                }
                waypointScript.On = false;
                waypointScript.waypointIndex = 0;
                waypointScript.waypoints.Clear();
                waypointScript.waypoints.Add(rb.AssignTarget());
                waypointScript.waypoints.Add(other.gameObject);
                waypointScript.On = true;
            }
        }

        if (other.CompareTag("NGO"))
        {
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {
                n.Civilians += 1f;
                Civilian = true;

                n.Casualties += 1f;
                n.Supply -= 1f;

                //Debug.Log(transform.name + " attacks " + other.name);
            }
        }

        if (other.CompareTag("Village"))
        {
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {
                 v.Casualties += 1f;
                 v.Supply -= 1f;

                 //Debug.Log(transform.name + " attacks " + other.name);
            }
        }
    }
}
