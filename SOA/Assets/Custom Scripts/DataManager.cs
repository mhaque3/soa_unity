using UnityEngine;
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
            Debug.Log("DataManager: Received belief of type "
                + (int) b.getBeliefType() + "\n" + b);
        }
    }
}
