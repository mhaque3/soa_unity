using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaActor : MonoBehaviour 
{
    public int unique_id;
    public int affiliation;
    public int type;

    public SoaSensor[] Sensors;
    public List<GameObject> Detections;
    public List<GameObject> Tracks;
	// Use this for initialization
	void Start () 
    {
        Sensors = transform.GetComponentsInChildren<SoaSensor>();

        foreach (SoaSensor sensor in Sensors)
        {
            sensor.soaActor = this;
        }
	}

    // Update is called once per frame
    void Update() 
    {
	
	}
}
