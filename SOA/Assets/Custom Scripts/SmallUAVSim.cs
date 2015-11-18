using UnityEngine;
using System.Collections;

public class SmallUAVSim : MonoBehaviour 
{
    SoaActor thisSoaActor;
    public float fuelTankSize_s;

    // Awake is called first before anything else
    void Awake()
    {
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // Start on a full tank
        thisSoaActor.fuelRemaining_s = fuelTankSize_s;
    }

	// Use this for initialization upon activation
	void Start () 
    {
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
