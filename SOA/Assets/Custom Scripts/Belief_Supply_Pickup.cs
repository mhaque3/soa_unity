using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Supply_Pickup : Belief
    {
        // Members
        private ulong request_time;
        private int actor_id;
        private bool greedy;
        private int multiplicity;

        // Constructor
        public Belief_Supply_Pickup(ulong request_time, int actor_id, bool greedy, int multiplicity)
            : base(actor_id)
        {
            this.request_time = request_time;
            this.actor_id = actor_id;
            this.greedy = greedy;
            this.multiplicity = multiplicity;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.SUPPLY_PICKUP;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Supply_Pickup {"
                + "\n" + "  request_time: " + request_time
                + "\n" + "  actor_id: " + actor_id
                + "\n" + "  greedy: " + greedy
                + "\n" + "  multiplicity: " + multiplicity
                + "\n" + "}";
            return s;
        }

        // Get methods
        public ulong getRequest_time() { return request_time; }
        public int getActor_id() { return actor_id; }
        public bool getGreedy() { return greedy; }
        public int getMultiplicity() { return multiplicity; }
    }
}
