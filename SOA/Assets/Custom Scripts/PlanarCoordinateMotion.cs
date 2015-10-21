using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class PlanarCoordinateMotion : MonoBehaviour 
{
    public bool On;
    public bool GroundHugger = false;
    public float AltitudeOffset;
    public float speed;
    public float WaypointSlowFactor;
    float currentSpeed;
    public float maxTurn;
    public float waypointEpsilon;
    public List<PrimitivePair<float,float>> waypoints;
    public int waypointIndex;
    public bool TeleportFromLastWaypoint = false;

    public Vector3 targetPosition;

    public RaycastHit hit;

	// Use this for initialization
	protected void Start () 
    {
        waypointIndex = 0;
        currentSpeed = 0;

        transform.position = new Vector3(transform.position.x, AltitudeOffset, transform.position.z);

        if(GetComponent<Animation>() != null)
            GetComponent<Animation>().wrapMode = WrapMode.Loop;
	}
	
	// Update is called once per frame
    protected float timeInterval = 0;
	protected void Update () 
    {
        if (On)
        {
            float dt = Time.deltaTime;
            float motion = currentSpeed * dt;
            transform.Translate(motion * Vector3.forward);

            timeInterval += dt;

            if (timeInterval > .01)
            {
                Vector3 deltaV;

                if (waypoints.Count > 0)
                {
                    if (targetPosition == null)
                    {
                        targetPosition = new Vector3();
                    }
                    targetPosition.x = waypoints[waypointIndex].first;
                    targetPosition.y = AltitudeOffset;
                    targetPosition.z = waypoints[waypointIndex].second;

                    deltaV = (new Vector3(targetPosition.x, transform.position.y, targetPosition.z)) - transform.position;
                    if (deltaV.x*deltaV.x + deltaV.z*deltaV.z < waypointEpsilon*waypointEpsilon)
                    {
                        waypointIndex++;
                        if (waypointIndex >= waypoints.Count)
                        {
                            waypointIndex = 0;
                            if (TeleportFromLastWaypoint)
                            {
                                transform.position = new Vector3(waypoints[0].first, AltitudeOffset, waypoints[0].second);
                            }
                        }
                        currentSpeed = speed / WaypointSlowFactor;
                    }
                    else
                    {
                        Vector3 newHeading = Vector3.RotateTowards(transform.forward, deltaV, maxTurn, 0.0f);
                        transform.rotation = Quaternion.LookRotation(newHeading);
                    }
                }
 
                currentSpeed = Mathf.Lerp(currentSpeed, speed, dt / 3);

                timeInterval = 0;
            }

            if (GroundHugger)
            {
                Vector3 curNormal = Vector3.up;
                float curDir = transform.eulerAngles.y;

                transform.position = new Vector3(transform.position.x, UnityEngine.Terrain.activeTerrain.SampleHeight(transform.position) + .2f, transform.position.z);

                if (Physics.Raycast(transform.position, -curNormal, out hit))
                {
                    curNormal = Vector3.Lerp(curNormal, hit.normal, 30 * Time.deltaTime);
                    Quaternion grndTilt = Quaternion.FromToRotation(Vector3.up, curNormal);
                    transform.rotation = grndTilt * Quaternion.Euler(0, curDir, 0);
                }
            }
        }
	}

    public void AddWaypoint(PrimitivePair<float,float> newWaypoint)
    {
        if (waypoints == null)
        {
            waypoints = new List<PrimitivePair<float, float>>();
        }
        waypoints.Add(newWaypoint);
    }
}
