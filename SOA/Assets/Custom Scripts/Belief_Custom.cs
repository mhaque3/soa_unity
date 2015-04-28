using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Custom : Belief
    {
        // Members
        private byte[] data;

        // Constructor
        public Belief_Custom(byte[] data)
            : base(0)
        {
            this.data = (byte[]) data.Clone();
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.CUSTOM;
        }

        public byte[] getData()
        {
            return (byte[]) data.Clone();
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Custom {" + 
                "\n  data: " + System.Text.Encoding.UTF8.GetString(data) + 
                "\n}";
            return s;
        }
   }
}
