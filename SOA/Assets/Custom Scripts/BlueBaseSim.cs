using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class BlueBaseSim : MonoBehaviour 
{
    SoaSite thisSoaSite;
    SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> beliefDictionary;
    public float Casualties;
    public float SupplyRate;
    public float Supply;

    // Use this for initialization
    void Start()
    {
        thisSoaSite = gameObject.GetComponent<SoaSite>();
    }

    float simTimer;
    public float simInterval;
    // Update is called once per frame
    void Update()
    {
        simTimer += Time.deltaTime;
        if (simTimer > simInterval)
        {
            // Update resource count
            Supply += SupplyRate;
            simTimer = 0f;
        }
    }
}
