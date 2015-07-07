using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using soa;

public class SoaSite : MonoBehaviour
{
    public int unique_id;
    public double commsRange;
    public DataManager dataManager;
    protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> beliefDictionary;
    private static System.DateTime epoch = new System.DateTime(1970, 1, 1);
    // Use this for initialization
    void Start()
    {
        // Initialize the belief dictionary
        beliefDictionary = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
        beliefDictionary[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
        beliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Check if belief is newer than current belief of matching type and id, if so,
    // replace old belief with b.
    public void addBelief(Belief b)
    {
#if(UNITY_STANDALONE)
        //Debug.Log("SoaSite - DataManager: Received belief of type " + (int)b.getBeliefType() + "\n" + b);
#else
        Console.WriteLine("SoaSite - DataManager: Received belief of type "
            + (int)b.getBeliefType() + "\n" + b);
#endif

        SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
        if (tempTypeDict != null)
        {
            Belief oldBelief;
            if (!beliefDictionary[b.getBeliefType()].TryGetValue(b.getId(), out oldBelief) || oldBelief.getBeliefTime() < b.getBeliefTime())
            {
                beliefDictionary[b.getBeliefType()][b.getId()] = b;
            }
        }
        else
        {
            beliefDictionary[b.getBeliefType()][b.getId()] = b;
        }
    }

    public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> getBeliefDictionary()
    {
        return beliefDictionary;
    }

    public void broadcastComms()
    {
        //Broadcast types ACTOR, MODE_COMMAND, SPOI, WAYPOINT, WAYPOINT_OVERRIDE, BASE, NGOSITE, VILLAGE
        publishBeliefsOfType(Belief.BeliefType.ACTOR);
        publishBeliefsOfType(Belief.BeliefType.MODE_COMMAND);
        publishBeliefsOfType(Belief.BeliefType.SPOI);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT);
        publishBeliefsOfType(Belief.BeliefType.WAYPOINT_OVERRIDE);
        publishBeliefsOfType(Belief.BeliefType.BASE);
        publishBeliefsOfType(Belief.BeliefType.NGOSITE);
        publishBeliefsOfType(Belief.BeliefType.VILLAGE);
    }

    private void publishBeliefsOfType(Belief.BeliefType type)
    {
        ulong currentTime = (ulong)(System.DateTime.UtcNow - epoch).Milliseconds;
        if (beliefDictionary.ContainsKey(type))
        {
            SortedDictionary<int, Belief> typeDict = beliefDictionary[type];
            foreach (KeyValuePair<int, Belief> entry in typeDict)
            {
                //only publish new data
                if (entry.Value.getBeliefTime() < (UInt64)(System.DateTime.UtcNow - epoch).Ticks / 10000 - 5000)
                {
                    if (dataManager != null)
                        dataManager.addAndBroadcastBelief(entry.Value, entry.Key);
                }
            }
        }
    }

    // Get current time
    // Note: This is a very large number.  When comparing (subtracting) times, do all math in
    // ulong and then cast at end to float as needed.  Do NOT cast to float before subtracting.
    public ulong getCurrentTime_ms()
    {
        return (ulong)(System.DateTime.UtcNow - epoch).Ticks / 10000;
    }
}
