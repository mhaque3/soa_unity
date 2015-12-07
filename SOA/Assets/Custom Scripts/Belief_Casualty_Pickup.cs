using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Casualty_Pickup : Belief
    {
        // Members
        private ulong request_time;
        private int actor_id;
        private bool greedy;
        private int[] ids;
        private int[] multiplicity;

        // Constructor
        public Belief_Casualty_Pickup(ulong request_time, int actor_id, bool greedy, int[] ids, int[] multiplicity)
            : base(actor_id)
        {
            this.request_time = request_time;
            this.actor_id = actor_id;
            this.greedy = greedy;
            this.ids = new int[ids.Length];
            Array.Copy(ids, this.ids, ids.Length);
            this.multiplicity = new int[multiplicity.Length];
            Array.Copy(multiplicity, this.multiplicity, multiplicity.Length);
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.CASUALTY_PICKUP;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Casualty_Pickup {"
                + "\n" + "  request_time: " + request_time
                + "\n" + "  actor_id: " + actor_id
                + "\n" + "  greedy: " + greedy
                + "\n" + "  [";
            int numEntries = (ids.Length < multiplicity.Length) ? ids.Length : multiplicity.Length;
            for (int i = 0; i < numEntries; i++)
            {
                s += "\n" + "    " + ids[i] + " -> " + multiplicity[i];
            }
            s += "\n" + "  ]\n}";
            return s;
        }

        // Get methods
        public ulong getRequest_time() { return request_time; }
        public int getActor_id() { return actor_id; }
        public bool getGreedy() { return greedy; }
        public int[] getIds()
        {
            // Return a copy, not reference to internal data
            int[] returnArray = new int[ids.Length];
            Array.Copy(ids, returnArray, ids.Length);
            return returnArray;
        }
        public int[] getMultiplicity()
        {
            // Return a copy, not reference to internal data
            int[] returnArray = new int[multiplicity.Length];
            Array.Copy(multiplicity, returnArray, multiplicity.Length);
            return returnArray;
        }
    }
}
