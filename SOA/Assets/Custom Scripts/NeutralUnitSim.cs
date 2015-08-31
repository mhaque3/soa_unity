using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NeutralUnitSim : MonoBehaviour 
{
    SimControl simControlScript;
    SoldierWaypointMotion waypointScript;
    NavMeshAgent thisNavAgent;
    GameObject currDestination;
    List<GameObject> choices;
    SoaActor thisSoaActor;

	// Use this for initialization
	void Start () 
    {
        // Get references to scripts
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        waypointScript = gameObject.GetComponent<SoldierWaypointMotion>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // To hold source/destination choices during random selection
        choices = new List<GameObject>();

        // Randomly assign a destination
        currDestination = ChooseRandomNeutralSite();
        OverrideWaypoint(currDestination);
	}
	
	// Update is called once per frame
    public float PathUpdateInterval;
    float PathUpdateClock = 0f;
	void Update ()
    {
        float dt = Time.deltaTime;
        PathUpdateClock += dt;
        if (PathUpdateClock > PathUpdateInterval)
        {
            thisNavAgent.ResetPath();
            PathUpdateClock = Random.value * PathUpdateInterval * 0.5f;
        }
    }

    GameObject ChooseRandomNeutralSite()
    {
        // Count the number of places to choose from
        choices.Clear();
        choices.AddRange(simControlScript.Villages);
        choices.AddRange(simControlScript.NgoSites);

        // Log error if we do not have at least 2 choices
        if (choices.Count < 2)
        {
            Debug.LogError("NeutralTruckSim.ChooseRandomNeutral(): # of NGO Sites + Villages must be >= 2");
        }

        // Choose sites with uniform probability (Random.Range upper limit is exclusive)
        return choices[Mathf.FloorToInt(Random.Range(0.0f, (float)choices.Count))];
    }

    void OnTriggerEnter(Collider other)
    {
        // Determine if we need to choose a new source/destination pair
        bool chooseNewSourceDestination = false;
        if (other.CompareTag("NGO") && other.gameObject.name.Equals(currDestination.name))
        {
            chooseNewSourceDestination = true;
        }
        else if (other.CompareTag("Village") && other.gameObject.name.Equals(currDestination.name))
        {
            chooseNewSourceDestination = true;
        }

        // If so, then randomly choose a pair where the source and destination are not the same
        if (chooseNewSourceDestination)
        {
            // Warp agent to new location
            GameObject warpLocation = WarpNewLocation();

            // Assign a new destination that is different from where we warped
            currDestination = warpLocation;
            while (currDestination.name.Equals(warpLocation.name))
            {
                currDestination = ChooseRandomNeutralSite();
                OverrideWaypoint(currDestination);
            }

            // Give the agent a new ID
            simControlScript.relabelLocalNeutralActor(thisSoaActor.unique_id);
        }
    }

    void OverrideWaypoint(GameObject g)
    {
        waypointScript.On = false;
        thisNavAgent.ResetPath();
        waypointScript.waypointIndex = 0;
        waypointScript.waypoints.Clear();
        waypointScript.waypoints.Add(currDestination);
        waypointScript.On = true;
    }

    GameObject WarpNewLocation()
    {
        // Choose a new location
        GameObject g = ChooseRandomNeutralSite();

        // Warp there
        thisNavAgent.Warp(g.transform.position);

        // Return place where we warped
        return g;
    }
}
