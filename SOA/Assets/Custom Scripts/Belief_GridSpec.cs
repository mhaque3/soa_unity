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
        private float gridOrigin_x;
        private float gridOrigin_z;
        private float gridToWorldScale;

        // Constructor
        public Belief_GridSpec(int width, int height,
            float gridOrigin_x, float gridOrigin_z, float gridToWorldScale)
            : base(0)
        {
            this.width = width;
            this.height = height;
            this.gridOrigin_x = gridOrigin_x;
            this.gridOrigin_z = gridOrigin_z;
            this.gridToWorldScale = gridToWorldScale;
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
                + "\n" + "  gridOrigin_x: " + gridOrigin_x
                + "\n" + "  gridOrigin_z: " + gridOrigin_z
                + "\n" + "  gridToWorldScale: " + gridToWorldScale
                + "\n" + "}";
            return s;
        }

        // Get methods
        public int getWidth() { return width; }
        public int getHeight() { return height; }
        public float getGridOrigin_x() { return gridOrigin_x; }
        public float getGridOrigin_z() { return gridOrigin_z; }
        public float getGridToWorldScale() { return gridToWorldScale; }
    }
}
