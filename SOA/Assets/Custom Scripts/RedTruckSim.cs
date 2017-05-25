using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class RedTruckSim : MonoBehaviour
{
    SimControl simControlScript;
    SoldierWaypointMotion waypointScript;
    NavMeshAgent thisNavAgent;
    SoaActor thisSoaActor;
    public GameObject CivilianIcon;
    public List<GameObject> fakeTargets;

    GameObject target;
    GameObject closestBaseFromTarget;

    // Rails behavior
    bool rails;
    GameObject railsTarget;
    bool assignRailsTarget = true;  // Red base's first assignment is stored, then assignRailsTarget is irreversibly set to false for rest of game 

    // Zigzag behavior
    GameObject fakeWaypoint;        // A GameObject is plopped in relation to actor's heading to achieve zigzag behavior
    bool zig = true;                // Heading toward target when true (as oppoed to closestBaseFromTarget)
    int zigzagIndex = 1;            // Indexes waypointScript during zigzag behavior
    float zigzagTimer = 5.00f;      // "Frequency" of swerves
    float zigzagAmplitude = 2.00f;  // Amplitude of swerves
    float zigzigOffRadius = 4.00f;  // Stop zigzag behavior when close to either targer or closestBaseFromTarget
    float zigzagStuck = 0.00f;      // Measures how long actor is stuck

    // Pump fake behavior
    bool insertFakeTarget = false;  // Not used, but interesting for future
    bool removeFakeTarget = false;  // Not used, but interesting for future

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

        fakeWaypoint = new GameObject();
    }

    // Update is called once per frame
    public float PathUpdateInterval;
    float PathUpdateClock = 0.00f;

    void Update()
    {
        float dt = Time.deltaTime;
        PathUpdateClock += dt;
        zigzagStuck += dt;

        CivilianIcon.SetActive(thisSoaActor.numCiviliansStored > 0);

        deceptiveBehaviorSelection();

    }

    /*
     *  Assign deceptive behavior based on predRedMovement level
     *  prescribed in soaSimConfig
     */
    void deceptiveBehaviorSelection()
    {
        switch (simControlScript.predRedMovement)
        {
            case (0):
                //On rails: ferry between a fixed target and the closestBaseFromTarget
                rails = true;
                break;

            case (1):
                //On "rails" + zigzag 
                rails = true;
                zigzag(zigzagAmplitude = Random.Range(2.00f, 3.00f), zigzagTimer);
                break;

            case (3):
                //Random target + zigzag
                rails = false;
                zigzag(zigzagAmplitude = Random.Range(2.00f, 3.00f), zigzagTimer);
                break;

            default:
                //What we've always had...
                rails = false;
                break;
        }
    }

    void zigzag(float amplitude, float timer)
    {
        if (target != null)
        {
            if (PathUpdateClock > timer)
            {
                /*
                 * If zig is true, actor is heading toward target
                 * vel is the vector pointing in the direction of the target or the closest base 
                 */
                Vector3 vel = new Vector3(0f, 0f, 0f);
                if (zig)
                {
                    vel = target.transform.position;
                }
                else
                {
                    vel = closestBaseFromTarget.transform.position;
                }
                vel = vel - thisSoaActor.transform.position;

                /*
                 * Zigzag behavior realized by plopping fakeWaypoint: vector transform.right scaled by (-1)^z 
                 */

                fakeWaypoint.transform.position = thisSoaActor.transform.position + (amplitude * Mathf.Pow(-1.00f, zigzagIndex) * thisSoaActor.transform.right) + amplitude/Random.Range(2.00f, 4.00f) * vel.normalized;
                /*
                 * Possible zigzag behavior through Mathf.Sin() -- not used 
                 */
                //fakeWaypoint.transform.position = thisSoaActor.transform.position + (4.00f * Mathf.Sin(PathUpdateClock) * thisSoaActor.transform.right) + vel.normalized;

                if (zig)
                {
                    waypointScript.waypoints.Insert(0, fakeWaypoint);
                }
                else
                {
                    waypointScript.waypoints.Insert(waypointScript.waypoints.Count - 1, fakeWaypoint);
                }
                PathUpdateClock = 0.00f;
                zigzagIndex++;
            }
        }
        // RefreshZigzag
        //refreshZigzag();
    }

    void refreshZigzag()
    {
        
        //if (zigzagStuck > PathUpdateInterval)
        //{
        //    waypointScript.waypoints.Clear();
        //    waypointScript.waypoints.Add(closestBaseFromTarget);
        //    zigzagStuck = 0.00f;
        //}
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

    // Calculate distance to target
    float CalculateDistTarget(GameObject v)
    {
        return (thisSoaActor.transform.position - v.transform.position).magnitude;
    }

    GameObject GetRetreatRedBase(BluePoliceSim threat)
    {
        GameObject selectedRedBase = simControlScript.RedBases[0];
        float minRangeRatio = float.MaxValue;
        foreach (GameObject redBase in simControlScript.RedBases)
        {
            NavMeshPath thisPath = new NavMeshPath();
            if (thisNavAgent.CalculatePath(redBase.transform.position, thisPath)) //true if result exists; result is stored in thisPath
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
        if (other.CompareTag("BluePolice")) // Killed by police
        {
            SoaSensor s = other.gameObject.GetComponentInChildren<SoaSensor>();
            if (s != null)
            {
                s.logKill(thisSoaActor);
            }

            // Log event
            simControlScript.soaEventLogger.LogRedTruckCaptured(other.name, gameObject.name);

            // Find out where to retreat to
            BluePoliceSim b = other.gameObject.GetComponent<BluePoliceSim>();
            Vector3 retreatBasePosition = GetRetreatRedBase(b).transform.position;

            // Destroy self
            simControlScript.DestroyRedTruck(gameObject);

            // Configure a replacement agent
            RedTruckConfig c = new RedTruckConfig(
                retreatBasePosition.x / SimControl.KmToUnity,
                thisSoaActor.simAltitude_km,
                retreatBasePosition.z / SimControl.KmToUnity,
                new Optional<int>(), // id (determined at runtime)
                new Optional<float>(), // beamwidth (use default)
                new Optional<string>(),
                new Optional<bool>(),
                new Optional<bool>(),
                new Optional<float>(),
                new Optional<float>(),
                new Optional<int>());

            // Instantiate and activate a replacement
            simControlScript.ActivateRedTruck(simControlScript.InstantiateRedTruck(c, true));
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

                // Assign a (target, closestBaseFromThatTarget) pair
                waypointScript.On = false;
                thisNavAgent.ResetPath();
                waypointScript.waypointIndex = 0;
                waypointScript.waypoints.Clear();

                // Nearest red base to intitial position assigns target
                target = rb.AssignTarget();

                // Store the initial target assignment in railsTarget, regardless of predRedMovement level 
                if (assignRailsTarget)
                {
                    railsTarget = target;
                    assignRailsTarget = false; //so that we don't do this again
                }

                // On rails, so set target back to initial assignment
                if (rails)
                {
                    target = railsTarget;
                }

                // Find red base closest to target
                closestBaseFromTarget = simControlScript.FindClosestInList(target, simControlScript.RedBases);

                // Add (target, closestBaseFromTarget) pair to waypointScript
                waypointScript.waypoints.Add(target);
                waypointScript.waypoints.Add(closestBaseFromTarget);
                waypointScript.On = true;

                // Weapon
                foreach (SoaWeapon weapon in thisSoaActor.Weapons)
                {
                    weapon.enabled = rb.EnableWeapon();
                }
            }
            // Indicate that actor is heading toward target
            zig = true;
        }

        if (other.CompareTag("NGO"))
        {
            // Red truck can inflict casualties, destroy supplies, and pick up civilians
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
            // Indicates that actor is heading toward the closestBaseFromTarget
            zig = false;
        }

        if (other.CompareTag("Village"))
        {
            // Red truck only inflicts casualties and destroys supplies at villages
            VillageSim v = other.gameObject.GetComponent<VillageSim>();
            if (v != null)
            {
                // Only inflict one casualty and take one supply
                v.Casualties += 1f;
                v.Supply = (v.Supply > 1f) ? (v.Supply - 1f) : 0f; // Can't go negative

                Debug.Log(transform.name + " attacks " + other.name);
            }
            // Indicates that actor is heading toward the closestBaseFromTarget
            zig = false;
        }
    }
}
