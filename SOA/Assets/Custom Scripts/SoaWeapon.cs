using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using soa;

[System.Serializable]
public class WeaponModality
{
    public string tagString;
    public bool enabled;
    public float RangeP1;
    public float RangeMax;
}

public class SoaWeapon : MonoBehaviour
{
    // Unity exposed parameters
    public WeaponModality[] modes;

    // Reference to soaActor and simControl
    SoaActor thisSoaActor;

    // Use this for initialization
    void Start()
    {
        // Save reference to my SoaActor
        thisSoaActor = gameObject.GetComponentInParent<SoaActor>();
    }

    public void UpdateWeapon(List<GameObject> detections)
    {
        foreach (GameObject target in detections)
        {
            // The object being detected must be alive
            if (target.GetComponent<SoaActor>().isAlive)
            {
                // Loop through all possible detect modes
                foreach (WeaponModality mode in modes)
                {
                    // Compute slant range and convert from unity units to km
                    Vector3 delta_unity = transform.position - target.transform.position;
                    float slantRange = delta_unity.magnitude / SimControl.KmToUnity;

                    // If this particular weapon mode is enabled and the game object matches its intended target
                    if (mode.enabled && mode.tagString == target.tag)
                    {
                        // Determine whether it was killed
                        if (slantRange <= mode.RangeP1)
                        {
                            // Successful kill, 100% probabiltiy
                            LogKill(target.gameObject);
                        }
                        else if (slantRange <= mode.RangeMax)
                        {
                            // Probability of being hit    
                            if (UnityEngine.Random.value < (mode.RangeMax - slantRange) / (mode.RangeMax - mode.RangeP1))
                            {
                                // Target was unlucky and was killed
                                LogKill(target.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }

    // Take actions to indicate to the scripts + user/GUI that target has been killed
    void LogKill(GameObject target)
    {
        // Tell the corresponding SoaActor that it has been killed so it no longer
        // sends out info or moves
        target.GetComponent<SoaActor>().Kill(gameObject.name);
    }

    // Update is called once per frame
    void Update() { }
}
