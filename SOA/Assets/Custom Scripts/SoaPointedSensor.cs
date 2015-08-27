using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaPointedSensor : SoaSensor {

    // Description
    public bool gimbaled;
    public float beamwidthDeg;
    public Vector3 boresightUnitVector;

    // Valid in all directions
    public override bool CheckSensorFootprint(GameObject target)
    {
        // Re-evaluate boresight if sensor is gimbaled
        if(gimbaled)
        {
            // Get the belief dictionary and SPOI Belief
            SortedDictionary<int, Belief> spoiBeliefDictionary = soaActor.getBeliefDictionary()[soa.Belief.BeliefType.SPOI];
            Belief belief;
            if (spoiBeliefDictionary.TryGetValue(soaActor.unique_id, out belief))
            {
                // Extract SPOI (x,z) in sim coordinates and transform to Unity coordinates
                Vector3 unitySpoi = new Vector3(
                    ((Belief_SPOI)belief).getPos_x() * SimControl.KmToUnity,
                    0,
                    ((Belief_SPOI)belief).getPos_y() * SimControl.KmToUnity);

                // Change sensor boresight vector
                boresightUnitVector = (unitySpoi - transform.position).normalized;
            }
        }

        // Compute relative vector
        Vector3 sensorToTargetUnitVector = (target.transform.position - transform.position).normalized;

        // Compute target cone angle relative to boresight 
        double coneAngleDeg = (180.0f / Mathf.PI) * Mathf.Acos(
            sensorToTargetUnitVector.x * boresightUnitVector.x + 
            sensorToTargetUnitVector.y * boresightUnitVector.y + 
            sensorToTargetUnitVector.z * boresightUnitVector.z);

        // Check if target is within beamwidthDeg/2 of boresight
        return coneAngleDeg <= (beamwidthDeg / 2);
    }
}
