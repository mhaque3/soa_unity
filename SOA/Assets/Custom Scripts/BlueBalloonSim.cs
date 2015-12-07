using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class BlueBalloonSim : MonoBehaviour 
{
    SoaActor thisSoaActor;

    // Awake is called first before anything else
    void Awake()
    {
        // Get pointer to SoaActor
        thisSoaActor = gameObject.GetComponent<SoaActor>();
    }

    // Use this for initialization upon activation
	void Start () 
    {
        // Unlimited fuel tank
        thisSoaActor.fuelRemaining_s = float.PositiveInfinity;
    }
}
