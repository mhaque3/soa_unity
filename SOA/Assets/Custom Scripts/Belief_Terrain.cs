using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Terrain : Belief
    {
        // Members
        private int type;
        private List<GridCell> cells;

        // Constructor
        public Belief_Terrain(int type, List<GridCell> cells)
            : base(0)
        {
            this.type = type;
            this.cells = GridCell.cloneList(cells);
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.TERRAIN;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Terrain {"
                + "\n" + "  type: " + type;
            for (int i = 0; i < cells.Count; i++)
            {
                s += "\n  " + cells[i];
            }
            s += "\n" + "}";
            return s;
        }

        // Get methods
        public int getType() { return type; }
        public List<GridCell> getCells() { return GridCell.cloneList(cells); }
    }
}
