using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Road : Belief
    {
        // Members
        private bool isRoadEnd;
        private float pos_x;
        private float pos_y;

        // Constructor
        public Belief_Road(bool isRoadEnd, float pos_x, float pos_y)
        {
            this.isRoadEnd = isRoadEnd;
            this.pos_x = pos_x;
            this.pos_y = pos_y;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.ROAD;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_BaseCell {"
                + "\n" + "  isRoadEnd: " + isRoadEnd
                + "\n" + "  pos_x: " + pos_x
                + "\n" + "  pos_y: " + pos_y
                + "\n" + "}";
            return s;
        }

        // Get methods
        public bool getIsRoadEnd() { return isRoadEnd; }
        public float getPos_x() { return pos_x; }
        public float getPos_y() { return pos_y; }
    }
}
