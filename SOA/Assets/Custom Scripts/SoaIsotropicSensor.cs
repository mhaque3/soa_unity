﻿using UnityEngine;
using System.Collections;

public class SoaIsotropicSensor : SoaSensor {

	// Valid in all directions
    public override bool CheckSensorFootprint(GameObject target)
    {
        Debug.LogError("nEED TO OVERRIDE");
        return true;
    }
}
