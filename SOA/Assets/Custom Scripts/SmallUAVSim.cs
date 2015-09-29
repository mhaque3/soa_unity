using UnityEngine;
using System.Collections;

public class SmallUAVSim : MonoBehaviour 
{
    SoaActor thisSoaActor;
    public float fuelTankSize;

	// Use this for initialization
	void Start () 
    {
        thisSoaActor = gameObject.GetComponent<SoaActor>();

        // Start on a full tank
        thisSoaActor.fuelRemaining = fuelTankSize;
	}
	
	// Update is called once per frame
	void Update () 
    {
        // Decrement fuel accordingly
        if (thisSoaActor.isAlive)
        {
            thisSoaActor.fuelRemaining -= Time.deltaTime;
            if (thisSoaActor.fuelRemaining <= 0)
            {
                // Killed by fuel
                thisSoaActor.Kill("Fuel");
                thisSoaActor.fuelRemaining = 0;
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlueBase"))
        {
            // Instant refuel
            thisSoaActor.fuelRemaining = fuelTankSize;
        }
    }
}
