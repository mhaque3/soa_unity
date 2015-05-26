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

	// Use this for initialization
	void Start () 
    {
        // Get references to scripts
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        waypointScript = gameObject.GetComponent<SoldierWaypointMotion>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();

        // To hold source/destination choices during random selection
        choices = new List<GameObject>();

        // Randomly assign an initial source/destination
        SetSourceDestinationPair();
	}
	
	// Update is called once per frame
	void Update () {}

    GameObject ChooseRandomNeutral()
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
            SetSourceDestinationPair();
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

    void SetSourceDestinationPair()
    {
        // Find the pair
        GameObject source = ChooseRandomNeutral();
        currDestination = source;
        while (source.name.Equals(currDestination.name))
        {
            currDestination = ChooseRandomNeutral();
        }

        // Set the new waypoint destination
        OverrideWaypoint(currDestination);

        // Teleport unit to the new source
        thisNavAgent.Warp(source.transform.position);
    }
}
