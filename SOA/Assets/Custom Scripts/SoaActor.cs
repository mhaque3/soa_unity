using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaActor : MonoBehaviour 
{
    public int unique_id;
    public int affiliation;
    public int type;

    public double commsRange;

    public SoaSensor[] Sensors;
    public List<GameObject> Detections;
    public List<GameObject> Tracks;

    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;
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

    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public void addBelief(Belief b)
    {
#if(NOT_UNITY)
            Console.WriteLine("DataManager: Received belief of type "
                + (int)b.getBeliefType() + "\n" + b);
#else
        Debug.Log("SoaActor - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);

        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        if (tempTypeDict != null)
        {
            Belief oldBelief = beliefDictionary[b.getBeliefType()][b.getId()];
            if (oldBelief == null || oldBelief.getTime() < b.getTime())
            {
                beliefDictionary[b.getBeliefType()][b.getId()] = b;
            }
        }
        else
        {
            beliefDictionary[b.getBeliefType()][b.getId()] = b;
        }
#endif
    }

    public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> getBeliefDictionary()
    {
        return beliefDictionary;
    }

}
