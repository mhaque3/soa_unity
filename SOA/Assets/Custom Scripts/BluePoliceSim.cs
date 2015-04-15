using UnityEngine;
using System.Collections;

public class BluePoliceSim : MonoBehaviour 
{

    NavMeshAgent thisNavAgent;
    SimControl simControlScript;
	// Use this for initialization
	void Start () 
    {
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public float GetRangeTo(GameObject destination)
    {
        NavMeshPath thisPath = new NavMeshPath();
        if (thisNavAgent.CalculatePath(destination.transform.position, thisPath))
        {
            return PathLength(thisPath);
        }
        else
        {
            return -1f;
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

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(transform.name + " collides with " + other.name);

        if (other.CompareTag("RedTruck"))
        {
            RedTruckSim r = other.gameObject.GetComponent<RedTruckSim>();
            if (r != null)
            {
                Debug.Log(transform.name + " intercepts " + other.name);
            }
        }

        if (other.CompareTag("RedDismount"))
        {
            RedDismountSim r = other.gameObject.GetComponent<RedDismountSim>();
            if (r != null)
            {
                Debug.Log(transform.name + " intercepts " + other.name);
            }
        }

        if (other.CompareTag("NGO"))
        {
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {

            }
        }

        if (other.CompareTag("Village"))
        {
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {

            }
        }
    }
}
