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

        // NGO site cells
        public List<PrimitivePair<int, int>> ngoSiteCells;

        // Village cells
        public List<PrimitivePair<int, int>> villageCells;

        // Blue base cells
        public List<PrimitivePair<int, int>> blueBaseCells;

        // Red base cells
        public List<PrimitivePair<int, int>> redBaseCells;

        // Constructor
        public EnvConfig()
        {
            // Initialize lists
            landCells = new List<PrimitivePair<int, int>>();
            waterCells = new List<PrimitivePair<int, int>>();
            mountainCells = new List<PrimitivePair<int, int>>();
            roadCells = new List<PrimitivePair<int, int>>();
            ngoSiteCells = new List<PrimitivePair<int, int>>();
            villageCells = new List<PrimitivePair<int, int>>();
            blueBaseCells = new List<PrimitivePair<int, int>>();
            redBaseCells = new List<PrimitivePair<int, int>>();
        }
    }
}
