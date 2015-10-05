using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class SoaSensor : MonoBehaviour 
{
    public PerceptionModality[] modes;
    public SoaActor soaActor;
    public List<GameObject> possibleDetections;
    //public float UpdateRate = 1f;
	// Use this for initialization
	void Start () 
    {
        soaActor = gameObject.GetComponentInParent<SoaActor>();
	}

	void Update () 
    {
	}

    abstract public bool CheckSensorFootprint(GameObject target);

    public void CheckDetections(List<GameObject> targets)
    {
        foreach (GameObject target in targets)
        {
            // The object being detected must be alive and within the sensor's footprint
            if (target.GetComponent<SoaActor>().isAlive && 
                CheckSensorFootprint(target))
            {
                // Loop through all possible detect modes
                foreach (PerceptionModality mode in modes)
                {
                    Vector3 delta = transform.position - target.transform.position;
                    float slantRange = delta.magnitude / SimControl.KmToUnity;

                    if (mode.tagString == target.tag)
                    {
                        if (slantRange < mode.RangeMax)
                        {
                            if (slantRange < mode.RangeP1)
                            {
                                // Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                                LogDetection(target.gameObject);
                            }
                            else
                            {
                                if (Random.value < (mode.RangeMax - slantRange) / (mode.RangeMax - mode.RangeP1))
                                {
                                    // Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                                    LogDetection(target.gameObject);
                                }
                                else
                                {
                                    // Debug.Log(soaActor.name + " failed detect of " + target.name + " at " + slantRange + "km");
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public void logKill(SoaActor killedActor)
    {
        soaActor.killDetections.Add(new soa.Belief_Actor(
            killedActor.unique_id, (int)killedActor.affiliation, killedActor.type, 
            false, (int)killedActor.isCarrying, killedActor.isWeaponized, killedActor.fuelRemaining,
            killedActor.transform.position.x / SimControl.KmToUnity,
            killedActor.transform.position.y / SimControl.KmToUnity,
            killedActor.transform.position.z / SimControl.KmToUnity));
    }

    void LogDetection(GameObject detectedObject)
    {
        if (soaActor.Detections.IndexOf(detectedObject) == -1)
        {
            soaActor.Detections.Add(detectedObject);
        }
    }
}
