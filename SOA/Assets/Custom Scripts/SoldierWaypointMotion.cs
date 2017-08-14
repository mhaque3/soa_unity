using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoldierWaypointMotion : MonoBehaviour
{
    public bool On;
    public float RoadCost = 10;
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
        if(navAgent != null)
            navAgent.SetAreaCost(NavMesh.GetAreaFromName("Road"), RoadCost);

        if (waypoints.Count < 1)
        {
            // Add current location as waypoint
            waypoints.Add(gameObject);

            // Set destination as target waypoint position
            targetPosition = waypoints[waypointIndex].transform.position;
            if(navAgent != null)
            navAgent.SetDestination(targetPosition);
        }
        else
        {
            // Set destination as target waypoint position
            targetPosition = waypoints[waypointIndex].transform.position;
            //navAgent.baseOffset = waypoints[waypointIndex].transform.position.y;
            if(navAgent != null)
                navAgent.SetDestination(targetPosition);
        }
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
                    targetPosition = waypoints[waypointIndex].transform.position;  // + new Vector3(0,transform.position.y,0);
                    if (navAgent != null)
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
                        if(navAgent != null)
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
        if (navAgent != null)
            return navAgent.speed;
        return 0.0f;
    }
}
