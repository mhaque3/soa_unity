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
    public float UpdateRate = 1f;
	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
    float updateClock = 0f;
	void Update () 
    {
        float dt = Time.deltaTime;
        updateClock += dt;
        if (updateClock > UpdateRate)
        {
            foreach (GameObject possible in possibleDetections)
            {
                foreach (Modality mode in modes)
                {
                    if (mode.tagString == possible.tag)
                    {
                        Vector3 delta = transform.position - possible.transform.position;
                        float slantRange = delta.magnitude;

                        if (slantRange < mode.RangeMax)
                        {
                            if (slantRange < mode.RangeP1)
                            {
                                LogDetection(possible);
                            }
                        }
                        else if (slantRange < mode.RangeMax)
                        {
                            if (((slantRange - mode.RangeP1) / (mode.RangeMax - mode.RangeP1)) < Random.value)
                            {
                                Debug.Log("Opportunity by " + transform.parent.name + " "
                                    + (slantRange - mode.RangeP1) / (mode.RangeMax - mode.RangeP1));
                                LogDetection(possible);
                            }
                        }

                    }
                }
            }
            possibleDetections.Clear();
            updateClock = 0f;
        }
	}

    void OnTriggerEnter(Collider other)
    {
    }

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
    }

    void OnTriggerExit(Collider other)
    {
        soaActor.Detections.Remove(other.gameObject);
        possibleDetections.Remove(other.gameObject);
    }

    void LogPossibleDetection(GameObject detectedObject)
    {
        if (possibleDetections.IndexOf(detectedObject) == -1)
            possibleDetections.Add(detectedObject);
    }

    void LogDetection(GameObject detectedObject)
    {
        if(soaActor.Detections.IndexOf(detectedObject) == -1)
            soaActor.Detections.Add(detectedObject);
    }
}
