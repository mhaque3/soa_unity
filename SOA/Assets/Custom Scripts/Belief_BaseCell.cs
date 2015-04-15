using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_BaseCell : Belief
    {
        // Members
        private int id;
        private float pos_x;
        private float pos_y;

        // Constructor
        public Belief_BaseCell(int id, float pos_x, float pos_y)
        {
            this.id = id;
            this.pos_x = pos_x;
            this.pos_y = pos_y;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.BASECELL;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_BaseCell {"
                + "\n" + "  id: " + id
                + "\n" + "  pos_x: " + pos_x
                + "\n" + "  pos_y: " + pos_y
                + "\n" + "}";
            return s;
        }

        // Get methods
	    public int getId(){ return id; }
        public float getPos_x() { return pos_x; }
        public float getPos_y() { return pos_y; }
    }
}
