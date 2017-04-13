using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaypointMotion : MonoBehaviour 
{
    private static double KM_PER_M = 1 / 1000.0;
    private static double REALSEC_PER_UNITYSEC = 60.0 / 1.0;

    public bool On;
    public bool GroundHugger = false;
    public float AltitudeOffset;
    public float speed_mps;//meters per second
    public float angle;
    public SoaActor actor;
    public float desiredAltitude_km;
    float currentSpeed;
    public float maxTurn;
    public float waypointEpsilon;
    public List<Waypoint> waypoints;
    public List<GameObject> displayedPoints;
    public GameObject prefabWaypoint;
    public int waypointIndex;
    public bool TeleportFromLastWaypoint = false;
    private soa.Belief lastBelief;
    public bool displayWaypoints;
    public RaycastHit hit;

    public WaypointMotion()
    {
        waypoints = new List<Waypoint>();
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

        actor = gameObject.GetComponent<SoaActor>();

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
                Waypoint target = GetTargetPosition();
                if (target != null)
                {
                    actor.SetDesiredAltitude(target.direction.y / SimControl.KmToUnity);
                    double speed_command = Math.Max(0, Math.Min(speed_mps, target.speed));
                    double unitySpeed = speed_command * KM_PER_M * REALSEC_PER_UNITYSEC * SimControl.KmToUnity;//UnityDistance / UnitySecond
                    currentSpeed = (float)unitySpeed;
                    Vector3 deltaV = (new Vector3(target.direction.x, target.direction.y, target.direction.z)) - transform.position;
                    deltaV.y = 0;//don't simulate changing height
                    if (deltaV.magnitude > 0.001)
                    {
                        deltaV.Normalize();
                        transform.rotation = Quaternion.LookRotation(new Vector3(deltaV.x, deltaV.y, deltaV.z));
                        transform.position = transform.position + (currentSpeed * timeInterval * deltaV);
                    }
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
                    Vector3 location = new Vector3(waypoint.x * SimControl.KmToUnity, 
                                                    waypoint.y * SimControl.KmToUnity, 
                                                    waypoint.z * SimControl.KmToUnity);
                    waypoints.Add(new Waypoint(location, waypoint.speed));
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
                Vector3 location = new Vector3(belief.getPos_x() * SimControl.KmToUnity, 
                                              belief.getPos_y() * SimControl.KmToUnity, 
                                               belief.getPos_z() * SimControl.KmToUnity);
                waypoints.Add(new Waypoint(location, belief.getSpeed()));
                waypointIndex = 0;
                lastBelief = belief;
                UpdateDisplay();
            }
        }
    }

    private Waypoint GetTargetPosition()
    {
        lock (waypoints)
        {
            if (waypoints.Count > 0 && WaypointIndexIsValid())
            {
                return GetTargetPositionFromPath();
            }
            
            return new Waypoint(transform.position, 0);
        }
    }

    private bool WaypointIndexIsValid()
    {
        return !(waypointIndex < 0 || waypointIndex >= waypoints.Count);
    }

    private Waypoint GetTargetPositionFromPath()
    {
        Waypoint target = waypoints[waypointIndex];

        Vector3 currentPosition = transform.position;
        currentPosition.y = actor.simAltitude_km * SimControl.KmToUnity;

        Vector3 deltaV = target.direction - currentPosition;
        //deltaV.y = 0;

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
                foreach(Waypoint location in waypoints)
                {
                    GameObject waypointDisplay = (GameObject)GameObject.Instantiate(prefabWaypoint, location.direction, Quaternion.identity);
                    displayedPoints.Add(waypointDisplay);
                }
            }
        }
    }

    public class Waypoint
    {
        public Vector3 direction { get; private set; }
        public float speed { get; private set; }

        public Waypoint(Vector3 direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
        }
    }
}
