using UnityEngine;
using System.Collections;

public class BlueBaseSim : MonoBehaviour 
{
    public float Casualties;
    public float SupplyRate;
    public float Supply;
    // Use this for initialization
    void Start()
    {

    }

    float simTimer;
    public float simInterval;
    // Update is called once per frame
    void Update()
    {
        simTimer += Time.deltaTime;
        if (simTimer > simInterval)
        {
            Supply += SupplyRate;

            simTimer = 0f;
        }
    }
}
