using UnityEngine;
using System.Collections;

public class NgoSim : MonoBehaviour 
{
    public float CasualtyRate;
    public float Casualties;
    public float CivilianRate;
    public float Civilians;
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
            Civilians += CivilianRate;
            Casualties += CasualtyRate;

            Supply -= SupplyRate;
            if (Supply < 0f)
                Supply = 0f;

            simTimer = 0f;
        }
    }
}
