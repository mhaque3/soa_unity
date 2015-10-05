using UnityEngine;
using System.Collections;

public class SmallUAVSim : MonoBehaviour 
{
    SoaActor thisSoaActor;
    public float fuelTankSize_s;

	// Use this for initialization
	void Start () 
    {
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // Start on a full tank
        thisSoaActor.fuelRemaining_s = fuelTankSize_s;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlueBase"))
        {
            // Instant refuel
            thisSoaActor.fuelRemaining_s = fuelTankSize_s;
        }
    }
}
