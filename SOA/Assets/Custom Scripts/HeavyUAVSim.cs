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
                    // Check if we are supposed to deliver a supply here
                    if (CheckSupplyDelivery(n.destination_id))
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
                if (Supply)
                {
                    // Check if we are supposed to deliver a supply here
                    if(CheckSupplyDelivery(v.destination_id)){
                        v.Supply += 1f;
                        Supply = false;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.NONE;

                        // Log event
                        simControlScript.soaEventLogger.LogSupplyDelivered(gameObject.name, other.name);
                    }
                }
                if (!Casuality && v.Casualties >= 1f)
                {
                    {
                        v.Casualties -= 1f;
                        Casuality = true;
                        thisSoaActor.isCarrying = SoaActor.CarriedResource.CASUALTIES;
                    }
                }
            }
        }
    }

    // Check if we are allowed to deliver supplies to the destination_id and remove it from the belief if it does exist
    private bool CheckSupplyDelivery(int destination_id)
    {
            // Get the belief dictionary and Supply_Delivery Belief
            SortedDictionary<int, Belief> supplyDeliveryBeliefDictionary = thisSoaActor.getBeliefDictionary()[soa.Belief.BeliefType.SUPPLY_DELIVERY];
            Belief belief;
            if (supplyDeliveryBeliefDictionary.TryGetValue(soaActor.unique_id, out belief))
            {
                Belief_Supply_Delivery bsd = (Belief_Supply_Delivery)belief;
                if (bsd.getDeliver_anywhere())
                {
                    // Deliver anywhere
                    return true;
                }
                else
                {
                    // Deliver to specific locations
                    int[] allowed_ids = bsd.getDestination_ids();

                    // Search for location of destination_id in array
                    int foundIdx = -1;
                    for (int i = 0; i < allowed_ids.Length; i++)
                    {
                        if (allowed_ids[i] == destination_id)
                        {
                            foundIdx = i;
                            break;
                        }
                    }

                    // Depending on if it was found or not
                    if (foundIdx >= 0)
                    {
                        // Location was found, create a new belief with that entry removed and return true
                        int[] new_allowed_ids = new int[allowed_ids.Length - 1];
                        int j = 0;
                        for (int i = 0; i < allowed_ids.Length; i++)
                        {
                            if (i != foundIdx)
                            {
                                new_allowed_ids[j++] = allowed_ids[i];
                            }
                        }
                        Belief_Supply_Delivery newBsd = new Belief_Supply_Delivery(
                            bsd.getRequest_time(), bsd.getActor_id(),
                            bsd.getDeliver_anywhere(), new_allowed_ids);
                        thisSoaActor.addBeliefToUnmergedBeliefDictionary(newBsd);
                        return true;
                    }
                    else
                    {
                        // Location was not found, leave belief as is and return false
                        return false;
                    }
                }
            }
            else
            {
                // No supply delivery belief, default behavior is to drop off no matter what
                return true;
            }
    }
}
