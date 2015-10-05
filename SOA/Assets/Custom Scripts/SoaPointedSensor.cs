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
                // Extract SPOI (x,y,z) in sim coordinates [km]
                Vector3 spoi_km = new Vector3(
                    ((Belief_SPOI)belief).getPos_x(),
                    ((Belief_SPOI)belief).getPos_y(),
                    ((Belief_SPOI)belief).getPos_z());

                // Update the sensor boresight vector
                boresightUnitVector.x = spoi_km.x - transform.position.x/SimControl.KmToUnity; // [km]
                boresightUnitVector.y = spoi_km.y - soaActor.simAltitude_km; // [km]
                boresightUnitVector.z = spoi_km.z - transform.position.z/SimControl.KmToUnity; // [km]
                boresightUnitVector.Normalize();
            }
        }

        // Compute relative vector
        SoaActor targetActor = target.GetComponent<SoaActor>();
        Vector3 sensorToTargetUnitVector;
        sensorToTargetUnitVector.x = (target.transform.position.x - transform.position.x) / SimControl.KmToUnity; // [km]
        sensorToTargetUnitVector.y = targetActor.simAltitude_km - soaActor.simAltitude_km; // [km]
        sensorToTargetUnitVector.z = (target.transform.position.z - transform.position.z) / SimControl.KmToUnity; // [km]
        sensorToTargetUnitVector.Normalize();

        // Compute target cone angle relative to boresight 
        double coneAngleDeg = (180.0f / Mathf.PI) * Mathf.Acos(
            sensorToTargetUnitVector.x * boresightUnitVector.x + 
            sensorToTargetUnitVector.y * boresightUnitVector.y + 
            sensorToTargetUnitVector.z * boresightUnitVector.z);

        // Check if target is within beamwidthDeg/2 of boresight
        return coneAngleDeg <= (beamwidthDeg / 2);
    }
}
