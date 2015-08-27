using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using soa;

[System.Serializable]
public class ClassifierModality
{
    public string tagString;
    public bool enabled;
    public float RangeP1;
    public float RangeMax;
}

public class SoaClassifier : MonoBehaviour
{
    // Unity exposed parameters
    public ClassifierModality[] modes;

    // Reference to soaActor
    SoaActor thisSoaActor;

    // Use this for initialization
    void Start()
    {
        // Save reference to my SoaActor
        thisSoaActor = gameObject.GetComponentInParent<SoaActor>();
    }

    public void UpdateClassifications(List<GameObject> detections)
    {
        SoaActor targetActor;
        foreach (GameObject target in detections)
        {
            // Save pointer to target's SoaActor
            targetActor = target.GetComponent<SoaActor>();

            // The object being detected must be alive, only classify if we haven't already
            if (targetActor.isAlive && !thisSoaActor.checkClassified(targetActor.unique_id))
            {
                // Loop through all possible classifier modes
                foreach (ClassifierModality mode in modes)
                {
                    // Compute slant range and convert from unity units to km
                    Vector3 delta_unity = transform.position - target.transform.position;
                    float slantRange = delta_unity.magnitude / SimControl.KmToUnity;

                    // If this particular classifier mode is enabled and the game object matches its intended target
                    if (mode.enabled && mode.tagString == target.tag)
                    {
                        // Determine whether it was classified
                        if (slantRange <= mode.RangeP1)
                        {
                            // Successful kill, 100% probabiltiy
                            thisSoaActor.setClassified(targetActor.unique_id);
                        }
                        else if (slantRange <= mode.RangeMax)
                        {
                            // Probability of being classified    
                            if (UnityEngine.Random.value < (mode.RangeMax - slantRange) / (mode.RangeMax - mode.RangeP1))
                            {
                                // Target was unlucky and was classified
                                thisSoaActor.setClassified(targetActor.unique_id);
                            }
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update() { }
}
