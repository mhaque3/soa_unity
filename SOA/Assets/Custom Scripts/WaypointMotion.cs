using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointMotion : MonoBehaviour 
{
    public bool On;
    public bool GroundHugger = false;
    public float AltitudeOffset;
    public float speed;
    public float angle;
    float currentSpeed;
    public float maxTurn;
    public float waypointEpsilon;
    public List<Vector3> waypoints;
    public List<GameObject> displayedPoints;
    public GameObject prefabWaypoint;
    public int waypointIndex;
    public bool TeleportFromLastWaypoint = false;
    private soa.Belief lastBelief;
    public bool displayWaypoints;
    public RaycastHit hit;

    public WaypointMotion()
    {
        waypoints = new List<Vector3>();
        displayedPoints = new List<GameObject>();
        On = false;
    }

    public void setDisplay(bool display)
    {
        if (displayWaypoints != display)
        {
            this.displayWaypoints = display;
            UpdateDisplay();
        }
    }

	// Use this for initialization
	protected void Start () 
    {
        waypointIndex = 0;
        currentSpeed = 0;
        On = false;
        displayWaypoints = false;

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
            timeInterval += dt;
                        
            if (timeInterval > .01)
            {
                Vector3 target = GetTargetPosition();
                if (target != null)
                {
                    currentSpeed = Mathf.Lerp(currentSpeed, speed, dt / 3);
                    Vector3 deltaV = (new Vector3(target.x, target.y, target.z)) - transform.position;
                    deltaV.y = 0;//don't simulate changing height
                    deltaV.Normalize();
                    transform.rotation = Quaternion.LookRotation(new Vector3(deltaV.x, deltaV.y, deltaV.z));
                    transform.position = transform.position + (currentSpeed * dt * deltaV);
                }
                else
                {
                    currentSpeed = 0;
                }
                
                timeInterval = 0;
            }
        }
	}

    public void SetWaypointBelief(soa.Belief_WaypointPath belief)
    {
        lock(waypoints)
        {
            if (belief != lastBelief && belief != null)
            {
                this.waypoints.Clear();
                
                foreach (soa.Waypoint waypoint in belief.getWaypoints())
                {
                    Vector3 location = new Vector3(waypoint.x, transform.position.y, waypoint.z);
                    waypoints.Add(location);
                }
                
                this.waypointIndex = 0;
                lastBelief = belief;

                UpdateDisplay();
            }
        }
    }

    public void SetWaypointBelief(soa.Belief_Waypoint belief)
    {
        lock (waypoints)
        {
            if (belief != lastBelief && belief != null)
            {
                this.waypoints.Clear();
                Vector3 location = new Vector3(belief.getPos_x(), transform.position.y, belief.getPos_z());
                waypoints.Add(location);
                waypointIndex = 0;
                lastBelief = belief;
                UpdateDisplay();
            }
        }
    }

    private Vector3 GetTargetPosition()
    {
        lock (waypoints)
        {
            if (waypoints.Count > 0 && WaypointIndexIsValid())
            {
                return GetTargetPositionFromPath();
            }
            
            return transform.position;
        }
    }

    private bool WaypointIndexIsValid()
    {
        return !(waypointIndex < 0 || waypointIndex >= waypoints.Count);
    }

    private Vector3 GetTargetPositionFromPath()
    {
        Vector3 target = waypoints[waypointIndex];

        Vector3 deltaV = target - transform.position;
        deltaV.y = 0;

        if (deltaV.magnitude < waypointEpsilon) {
            waypointIndex++;
        }

        return target;
    }

    private void UpdateDisplay()
    {
        lock(waypoints)
        {
            foreach (GameObject sphere in displayedPoints)
            {
                GameObject.Destroy(sphere);
            }
            this.displayedPoints.Clear();

            if (displayWaypoints && prefabWaypoint != null)
            {
                foreach(Vector3 location in waypoints)
                {
                    GameObject waypointDisplay = (GameObject)GameObject.Instantiate(prefabWaypoint, location, Quaternion.identity);
                    displayedPoints.Add(waypointDisplay);
                }
            }
        }
    }
}
