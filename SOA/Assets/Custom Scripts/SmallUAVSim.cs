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
    }

	// Use this for initialization upon activation
	void Start () 
    {
        // Start on a full tank
        thisSoaActor.fuelRemaining_s = fuelTankSize_s;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlueBase"))
        {
            // Instant refuel
            thisSoaActor.fuelRemaining_s = fuelTankSize_s;

            // Get an observation about the blue base
            BlueBaseSim b = other.gameObject.GetComponent<BlueBaseSim>();
            if (b != null)
            {
                thisSoaActor.RegisterSiteObservation(new soa.Belief_Base(
                    b.destination_id, b.gridCells, b.Supply));
            }
        }
        if (other.CompareTag("NGO"))
        {
            // Get an observation about the NGO
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {
                thisSoaActor.RegisterSiteObservation(new soa.Belief_NGOSite(
                    n.destination_id, n.gridCells, n.Supply, n.Casualties, n.Civilians));
            }
        }
        if (other.CompareTag("Village"))
        {
            // Get an observation about the village
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {
                thisSoaActor.RegisterSiteObservation(new soa.Belief_Village(
                    v.destination_id, v.gridCells, v.Supply, v.Casualties));
            }
        }
    }
}
