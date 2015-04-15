using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Time : Belief
    {
        // Members
        private ulong time;

        // Constructor
        public Belief_Time(ulong time)
        {
            this.time = time;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.TIME;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Time {"
                + "\n" + "  time: " + time
                + "\n" + "}";
            return s;
        }

        // Get methods
        public ulong getTime() { return time; }
    }
}
