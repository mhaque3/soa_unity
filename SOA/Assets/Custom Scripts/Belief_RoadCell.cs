using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_RoadCell : Belief
    {
        // Members
        private bool isRoadEnd;
        private GridCell cell;

        // Constructor
        public Belief_RoadCell(bool isRoadEnd, GridCell cell)
            : base(0)
        {
            this.isRoadEnd = isRoadEnd;
            this.cell = new GridCell(cell);
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.ROADCELL;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_RoadCell {"
                + "\n" + "  isRoadEnd: " + isRoadEnd
                + "\n  " + cell
                + "\n" + "}";
            return s;
        }

        // Get methods
        public bool getIsRoadEnd() { return isRoadEnd; }
        public GridCell getCell() { return new GridCell(cell); }
    }
}
