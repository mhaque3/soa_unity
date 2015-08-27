using UnityEngine;
using System.Collections;

public class SoaPointedSensor : SoaSensor {

    // Description
    public float beamwidthDeg;
    public Vector3 boresightUnitVector;

    // Valid in all directions
    public override bool CheckSensorFootprint(GameObject target)
    {
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
