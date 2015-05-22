using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoldierWaypointMotion : MonoBehaviour
{
    public bool On;
    public float RoadCost = 1;
    public float waypointEpsilon;
    public bool PATROL;
    public List<GameObject> waypoints;
    public int waypointIndex;
    public Vector3 targetPosition;
    NavMeshAgent navAgent;
    
    // Use this for initialization
	void Start () 
    {
        waypointIndex = 0;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.SetAreaCost(NavMesh.GetAreaFromName("Road"), RoadCost);

        targetPosition = waypoints[waypointIndex].transform.position;
        navAgent.SetDestination(targetPosition);
    }


 	// Update is called once per frame
    float timeInterval = 0;
	void Update () 
    {
        if (On)
        {
            float dt = Time.deltaTime;

            timeInterval += dt;

            if (timeInterval > .5)
            {
                Vector3 deltaV;

                if (waypoints.Count > waypointIndex)
                {
                    targetPosition = waypoints[waypointIndex].transform.position + new Vector3(0,transform.position.y,0);
                    navAgent.SetDestination(targetPosition);
                    deltaV = (new Vector3(targetPosition.x, targetPosition.y, targetPosition.z)) - transform.position;
                    if (deltaV.magnitude < waypointEpsilon)
                    {
                        waypointIndex++;
                        if (waypointIndex >= waypoints.Count)
                        {
                            waypointIndex = 0;
                        }
                        targetPosition = waypoints[waypointIndex].transform.position;
                        navAgent.SetDestination(targetPosition);

                    }
                    else
                    {
                        //Vector3 newHeading = Vector3.RotateTowards(transform.forward, deltaV, maxTurn, 0.0f);
                        //transform.rotation = Quaternion.LookRotation(newHeading);
                    }
                }


                timeInterval = 0;
            }
    	}
    }

    public float GetSpeed()
    {
        return navAgent.speed;
    }
}
