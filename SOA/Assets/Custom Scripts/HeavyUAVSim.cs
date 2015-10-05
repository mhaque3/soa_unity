using UnityEngine;
using System.Collections;

public class HeavyUAVSim : MonoBehaviour 
{
    SimControl simControlScript;
    SoaActor thisSoaActor;
    public float fuelTankSize_s;
    public bool Casuality;
    public bool Supply;
    public GameObject SupplyIcon;
    public GameObject CasualtyIcon;

	// Use this for initialization
	void Start () 
    {
        thisSoaActor = gameObject.GetComponent<SoaActor>();
        simControlScript = GameObject.FindObjectOfType<SimControl>();

        // Start on a full tank
        thisSoaActor.fuelRemaining_s = fuelTankSize_s;
	}
	
	// Update is called once per frame
	void Update () 
    {
        // Set icons
        SupplyIcon.SetActive(Supply);
        CasualtyIcon.SetActive(Casuality);
	}

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(transform.name + " collides with " + other.name);

        if (other.CompareTag("BlueBase"))
        {
            // Instant refuel
            thisSoaActor.fuelRemaining_s = fuelTankSize_s;

            // Process casualties/supplies
            BlueBaseSim b = other.gameObject.GetComponent<BlueBaseSim>();
            if (b != null)
            {
                if (Casuality)
                {
                    {
                        b.Casualties += 1f;
                        Casuality = false;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.NONE;

                        // Log event
                        simControlScript.soaEventLogger.LogCasualtyDelivery(gameObject.name, other.name);
                    }
                }
                if (!Supply && b.Supply >=1f)
                {
                    {
                        b.Supply -= 1f;
                        Supply = true;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.SUPPLIES;
                    }
                }
            }
        }

        if (other.CompareTag("NGO"))
        {
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {
                if (!Casuality && n.Casualties >= 1f)
                {
                    {
                        n.Casualties -= 1f;
                        Casuality = true;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.CASUALTIES;
                    }
                }
                if (Supply)
                {
                    {
                        n.Supply += 1f;
                        Supply = false;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.NONE;

                        // Log event
                        simControlScript.soaEventLogger.LogSupplyDelivered(gameObject.name, other.name);
                    }
                }
            }
        }

        if (other.CompareTag("Village"))
        {
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {
                if (!Casuality && v.Casualties >= 1f)
                {
                    {
                        v.Casualties -= 1f;
                        Casuality = true;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.CASUALTIES;
                    }
                }
                if (Supply)
                {
                    {
                        v.Supply += 1f;
                        Supply = false;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.NONE;

                        // Log event
                        simControlScript.soaEventLogger.LogSupplyDelivered(gameObject.name, other.name);
                    }
                }
            }
        }
    }
}
