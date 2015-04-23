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
        private float gridToUnityScale;
        private float gridOrigin_x;
        private float gridOrigin_z;

        // Constructor
        public Belief_GridSpec(int width, int height, float gridToUnityScale,
            float gridOrigin_x, float gridOrigin_z)
            : base(0)
        {
            this.width = width;
            this.height = height;
            this.gridToUnityScale = gridToUnityScale;
            this.gridOrigin_x = gridOrigin_x;
            this.gridOrigin_z = gridOrigin_z;
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
                + "\n" + "  gridToUnityScale: " + gridToUnityScale
                + "\n" + "  gridOrigin_x: " + gridOrigin_x
                + "\n" + "  gridOrigin_z: " + gridOrigin_z
                + "\n" + "}";
            return s;
        }

        // Get methods
        public int getWidth() { return width; }
        public int getHeight() { return height; }
        public float getGridToUnityScale() { return gridToUnityScale; }
        public float getGridOrigin_x() { return gridOrigin_x; }
        public float getGridOrigin_z() { return gridOrigin_z; }
    }
}
