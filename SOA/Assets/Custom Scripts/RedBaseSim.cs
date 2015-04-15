using UnityEngine;
using System.Collections;

public class RedBaseSim : MonoBehaviour 
{
    public float Civilians;
    SimControl simControlScript;
	// Use this for initialization
	void Start () 
    {
        simControlScript = GameObject.FindObjectOfType<SimControl>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public GameObject AssignTarget()
    {
        // totally random for now - we can get more clever later...

        int targetCount = simControlScript.NgoSites.Count + simControlScript.Villages.Count;
        int targetIndex = Random.Range(0, targetCount);

        if (targetIndex < simControlScript.NgoSites.Count)
        {
            return simControlScript.NgoSites[targetIndex];
        }
        else
        {
            return simControlScript.Villages[targetIndex - simControlScript.NgoSites.Count];
        }
    }
}
