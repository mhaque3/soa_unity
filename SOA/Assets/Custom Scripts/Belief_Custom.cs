using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Custom : Belief
    {
        // Members
        private int customType;
        private byte[] data;

        // Constructor
        public Belief_Custom(int customType, int actorID, byte[] data)
            : base(actorID)
        {
            this.customType = customType;
            this.data = (byte[]) data.Clone();
        }

        public int getCustomType()
        {
            return customType;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.CUSTOM;
        }

        public override Belief.Key getTypeKey()
        {
            return new Key(customType);
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
