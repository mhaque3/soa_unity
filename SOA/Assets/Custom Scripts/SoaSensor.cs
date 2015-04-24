using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class Modality
{
    public string tagString;
    public float RangeP1;
    public float RangeMax;
}

public class SoaSensor : MonoBehaviour 
{
    public Modality[] modes;
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

    public void CheckDetections(List<GameObject> targets)
    {
        foreach (GameObject target in targets)
        {
            foreach (Modality mode in modes)
            {
                Vector3 delta = transform.position - target.transform.position;
                float slantRange = delta.magnitude * SimControl.KmToUnity;

                if (mode.tagString == target.tag)
                {
                    if (slantRange < mode.RangeMax)
                    {
                        if (slantRange < mode.RangeP1)
                        {
                            Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                            LogDetection(target.gameObject);
                        }
                        else
                        {
                            if (Random.value < (mode.RangeMax - slantRange) / (mode.RangeMax - mode.RangeP1))
                            {
                                Debug.Log(soaActor.name + " detects " + target.name + " at " + slantRange + "km");
                                LogDetection(target.gameObject);
                            }
                            else
                            {
                                Debug.Log(soaActor.name + " failed detect of " + target.name + " at " + slantRange + "km");
                            }
                        }
                    }                  
                }
            }
        }
    }

    /*
    void OnTriggerStay(Collider other)
    {
        foreach(Modality mode in modes)
        {
            if (mode.tagString == other.gameObject.tag)
            {
                Vector3 delta = transform.position - other.transform.position;
                float slantRange = delta.magnitude;

                if (slantRange < mode.RangeMax)
                {
                    if (slantRange < mode.RangeP1)
                    {
                        LogDetection(other.gameObject);
                    }
                    else
                    {
                        LogPossibleDetection(other.gameObject);
                    }
                }
            }
        }
    }*/

    void LogDetection(GameObject detectedObject)
    {
        if (soaActor.Detections.IndexOf(detectedObject) == -1)
        {
            Debug.Log("Adding detection to soa actor list " + soaActor.unique_id);
            soaActor.Detections.Add(detectedObject);
        }
    }
}
