using UnityEngine;
using System.Collections;
using soa;

public class RedDismountSim : MonoBehaviour 
{
    SimControl simControlScript;
    SoldierWaypointMotion waypointScript;
    NavMeshAgent thisNavAgent;
    SoaActor thisSoaActor;
    public GameObject CivilianIcon;

    // Awake is called before anything else
    void Awake()
    {
        thisSoaActor = gameObject.GetComponent<SoaActor>();
        simControlScript = GameObject.FindObjectOfType<SimControl>();
        waypointScript = gameObject.GetComponent<SoldierWaypointMotion>();
        thisNavAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    // Use this for initialization upon activation
    void Start()
    {
        // Unlimited fuel tank
        thisSoaActor.fuelRemaining_s = float.PositiveInfinity;
    }

    // Update is called once per frame
    public float PathUpdateInterval;
    float PathUpdateClock = 0f;
    void Update()
    {
        float dt = Time.deltaTime;

        CivilianIcon.SetActive(thisSoaActor.numCiviliansStored > 0);

        PathUpdateClock += dt;
        if (PathUpdateClock > PathUpdateInterval)
        {
            thisNavAgent.ResetPath();
            PathUpdateClock = Random.value * PathUpdateInterval * 0.5f;
            //thisNavAgent.SetDestination(waypointScript.targetPosition);
        }
    }

    float PathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2)
            return 0f;

        Vector3 previousCorner = path.corners[0];
        float lengthSoFar = 0.0F;
        int i = 1;
        while (i < path.corners.Length)
        {
            Vector3 currentCorner = path.corners[i];
            lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
            i++;
        }
        return lengthSoFar;
    }

    GameObject GetRetreatRedBase(BluePoliceSim threat)
    {
        GameObject selectedRedBase = simControlScript.RedBases[0];
        float minRangeRatio = float.MaxValue;
        foreach (GameObject redBase in simControlScript.RedBases)
        {
            NavMeshPath thisPath = new NavMeshPath();
            if (thisNavAgent.CalculatePath(redBase.transform.position, thisPath))
            {
                float thisLength = PathLength(thisPath);
                float threatRange = threat.GetRangeTo(redBase);
                if (threatRange > 0f)
                {
                    float thisRatio = thisLength / threatRange;
                    if (thisRatio < minRangeRatio)
                    {
                        minRangeRatio = thisRatio;
                        selectedRedBase = redBase;
                    }
                }
            }
        }
        return selectedRedBase;
    }

    void OnTriggerEnter(Collider other)
    {
        // Return if triggered too early (SoaActor has not instantiated yet)
        if (thisSoaActor == null)
        {
            return;
        }

        if (other.CompareTag("BluePolice")) // Killed by police
        {
            SoaSensor s = other.gameObject.GetComponentInChildren<SoaSensor>();
            if (s != null)
            {
                s.logKill(thisSoaActor);
            }

            // Log event
            simControlScript.soaEventLogger.LogRedDismountCaptured(other.name, gameObject.name);

            // Find out where to retreat to
            BluePoliceSim b = other.gameObject.GetComponent<BluePoliceSim>();
            Vector3 retreatBasePosition = GetRetreatRedBase(b).transform.position;

            // Destroy self
            simControlScript.DestroyRedDismount(gameObject);

            // Configure a replacement agent
            RedDismountConfig c = new RedDismountConfig(
                retreatBasePosition.x / SimControl.KmToUnity,
                thisSoaActor.simAltitude_km,
                retreatBasePosition.z / SimControl.KmToUnity,
                new Optional<int>(), // id (determined at runtime)
                new Optional<float>(), // beamwidth (use default)
                new Optional<string>(),
                new Optional<bool>(),
                new Optional<float>(),
                new Optional<int>());

            // Instantiate and activate a replacement
            simControlScript.ActivateRedDismount(simControlScript.InstantiateRedDismount(c, true));
        }

        if (other.CompareTag("RedBase"))
        {
            RedBaseSim rb = other.gameObject.GetComponent<RedBaseSim>();
            if (rb != null)
            {
                // Drop off all civilians currently carried at the base
                for (int i = 0; i < thisSoaActor.numCiviliansStored; i++)
                {
                    simControlScript.soaEventLogger.LogCivilianInRedCustody(gameObject.name, other.name);
                }
                rb.Civilians += thisSoaActor.numCiviliansStored;
                thisSoaActor.numCiviliansStored = 0;

                // Assign a new target and return to closest base from that target
                waypointScript.On = false;
                thisNavAgent.ResetPath();
                waypointScript.waypointIndex = 0;
                waypointScript.waypoints.Clear();
                GameObject target = rb.AssignTarget();
                waypointScript.waypoints.Add(target);
                waypointScript.waypoints.Add(simControlScript.FindClosestInList(target, simControlScript.RedBases));
                waypointScript.On = true;

                foreach (SoaWeapon weapon in thisSoaActor.Weapons)
                {
                    weapon.enabled = rb.EnableWeapon();
                }
            }
        }

        if (other.CompareTag("NGO"))
        {
            // Red dismount can inflict casualties, destroy supplies, and pick up civilians
            // at NGO sites
            NgoSim n = other.gameObject.GetComponent<NgoSim>();
            if (n != null)
            {
                // Act greedy and pick up as many civilians as you have room for
                uint numFreeSlots = thisSoaActor.GetNumFreeSlots();
                n.Civilians += numFreeSlots; // Keeps track of civilians taken from this site
                thisSoaActor.numCiviliansStored += numFreeSlots;

                // Only inflict one casualty and take one supply
                n.Casualties += 1f;
                n.Supply = (n.Supply > 1f) ? (n.Supply - 1f) : 0f; // Can't go negative

                Debug.Log(transform.name + " attacks " + other.name);
            }
        }

        if (other.CompareTag("Village"))
        {
            // Red dismount only inflicts casualties and destroys supplies at villages
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {
                // Only inflict one casualty and take one supply
                v.Casualties += 1f;
                v.Supply = (v.Supply > 1f) ? (v.Supply - 1f) : 0f; // Can't go negative

                Debug.Log(transform.name + " attacks " + other.name);
            }
        }
    }
}
