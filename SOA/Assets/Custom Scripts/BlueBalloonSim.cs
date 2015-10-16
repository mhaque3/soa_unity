using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class BlueBalloonSim : MonoBehaviour 
{
    SoaActor thisSoaActor;

    // Use this for initialization
	void Start () 
    {
        // Get pointer to SoaActor
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // Unlimited fuel tank
        thisSoaActor.fuelRemaining_s = float.PositiveInfinity;
    }
}
