// Additinonal using statements are needed if we are running in Unity
#if(NOT_UNITY)
#else
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class DataManager
    {
        // Constructor
        public DataManager(){}

        // Dummy functions for filtering
        public bool filterBelief(Belief b)
        {
            // Everything passes for now
            return true;
        }

        // Dummy function for consuming beliefs
        public void addBelief(Belief b)
        {
            #if(NOT_UNITY)
            Console.WriteLine("DataManager: Received belief of type "
                + (int)b.getBeliefType() + "\n" + b);
            #else
            Debug.Log("DataManager: Received belief of type "
                + (int)b.getBeliefType() + "\n" + b);
            #endif
        }
    }
}
