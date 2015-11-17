using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class EnvConfig
    {
        // Gridmath fields
        public float gridOrigin_x;
        public float gridOrigin_z;
        public float gridToWorldScale;

        // Terrain cells
        public List<PrimitivePair<int, int>> landCells;
        public List<PrimitivePair<int, int>> waterCells;
        public List<PrimitivePair<int, int>> mountainCells;

        // Road cells
        public List<PrimitivePair<int, int>> roadCells;

        // Constructor
        public EnvConfig()
        {
            // Initialize lists
            landCells = new List<PrimitivePair<int, int>>();
            waterCells = new List<PrimitivePair<int, int>>();
            mountainCells = new List<PrimitivePair<int, int>>();
            roadCells = new List<PrimitivePair<int, int>>();
        }
    }
}
