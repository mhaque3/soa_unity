using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class RedUnitInfo
{
    public int id;
    public Vector3 pos;
    public Vector3 vel;
    public bool velIsValid;
    public bool hasCivilian;
    public bool isPursueCandidate;
    public float distToClosestProtectedSite;
    public float maxSpeed;
    public Vector3 closestProtectedSitePos;
    public float distToClosestRedBase;
    public Vector3 closestRedBasePos;

    public RedUnitInfo(Belief_Actor ba, List<GameObject> redUnits, List<GameObject> redBases, List<GameObject> protectedSites)
    {
        // Get ID
        id = ba.getUnique_id();

        // Extract 6 state (in Unity space)
        pos = new Vector3(
            ba.getPos_x() * SimControl.KmToUnity,
            ba.getPos_y() * SimControl.KmToUnity,
            ba.getPos_z() * SimControl.KmToUnity);
        vel = new Vector3(
            ba.getVelocity_x() * SimControl.KmToUnity,
            ba.getVelocity_y() * SimControl.KmToUnity,
            ba.getVelocity_z() * SimControl.KmToUnity);
        velIsValid = ba.getVelocity_x_valid() && ba.getVelocity_y_valid() && ba.getVelocity_z_valid();

        // Not a pursue candidate by default
        isPursueCandidate = false;

        // Compute distances
        computeProximityToProtectedSites(protectedSites);
        computeProximityToRedBases(redBases);

        // Look up Unity specific properties
        foreach (GameObject g in redUnits)
        {
            // Find the corresponding game object
            if (g.GetComponent<SoaActor>().unique_id == id)
            {
                // Save hasCivilian
                switch (g.tag)
                {
                    case "RedTruck":
                        hasCivilian = g.GetComponent<RedTruckSim>().Civilian;
                        break;
                    case "RedDismount":
                        hasCivilian = g.GetComponent<RedDismountSim>().Civilian;
                        break;
                    default:
                        // Unrecognized
                        Debug.LogError("RedUnitInfo(): Unrecognized g.tag " + g.tag);
                        break;
                }

                // Save max speed (in Unity coordinates)
                maxSpeed = g.GetComponent<NavMeshAgent>().speed;
            }
        }
    }

    public void computeProximityToProtectedSites(List<GameObject> protectedSites){
        // Loop through and find the closest protected iste
        distToClosestProtectedSite = float.PositiveInfinity;
        foreach (GameObject g in protectedSites)
        {
            float tempDist = Vector3.Distance(pos,g.transform.position);
            if (tempDist < distToClosestProtectedSite)
            {
                distToClosestProtectedSite = tempDist;
                closestProtectedSitePos = g.transform.position;
            }
        }
    }

    public void computeProximityToRedBases(List<GameObject> redBases)
    {
        // Loop through and find the closest red base
        distToClosestRedBase = float.PositiveInfinity;
        foreach (GameObject g in redBases)
        {
            float tempDist = Vector3.Distance(pos, g.transform.position);
            if (tempDist < distToClosestRedBase)
            {
                distToClosestRedBase = tempDist;
                closestRedBasePos = g.transform.position;
            }
        }
    }
}
