using UnityEngine;
using System.Collections;

public class HeavyLiftSim : MonoBehaviour 
{
    public bool Casuality;
    public bool Supply;
    public GameObject SupplyIcon;
    public GameObject CasualtyIcon;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        SupplyIcon.SetActive(Supply);
        CasualtyIcon.SetActive(Casuality);
	}

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(transform.name + " collides with " + other.name);

        if (other.CompareTag("BlueBase"))
        {
            BlueBaseSim b = other.gameObject.GetComponent<BlueBaseSim>();
            if (b != null)
            {
                if (Casuality)
                {
                    {
                        b.Casualties += 1f;
                        Casuality = false;
                    }
                }
                if (!Supply)
                {
                    {
                        b.Supply -= 1f;
                        Supply = true;
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
                    }
                }
                if (Supply)
                {
                    {
                        n.Supply += 1f;
                        Supply = false;
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
                    }
                }
                if (Supply)
                {
                    {
                        v.Supply += 1f;
                        Supply = false;
                    }
                }
            }
        }
    }
}
