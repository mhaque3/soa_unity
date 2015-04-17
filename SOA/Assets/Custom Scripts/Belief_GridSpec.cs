using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_GridSpec : Belief
    {
        // Members
        private int width;
        private int height;

        // Constructor
        public Belief_GridSpec(int width, int height)
            : base(0)
        {
            this.width = width;
            this.height = height;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.GRIDSPEC;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_GridSpec {"
                + "\n" + "  width: " + width
                + "\n" + "  height: " + height
                + "\n" + "}";
            return s;
        }

        // Get methods
        public int getWidth() { return width; }
        public int getHeight() { return height; }
    }
}
