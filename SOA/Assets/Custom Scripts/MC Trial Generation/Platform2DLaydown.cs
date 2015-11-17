using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Platform2DLaydown
    {
        public float numMean;
        public float numStdDev;
        public int numMin;
        public int numMax;
        public float fromAnchorStdDev_km; // Along x and z dimensions separately 
        public float fromAnchorMax_km; // Along x and z dimensions separately
        public List<PrimitivePair<float, float>> anchors;
        public HashSet<PrimitivePair<int, int>> allowedCells;

        public Platform2DLaydown() { }

        protected float RandN(float mean, float stdDev, float min, float max, Random rand)
        {
            bool firstTry = true;
            double u1, u2, randStdNormal, randNormal = 0;

            if (min == max)
            {
                // If bounds are equal, no need to draw number randomly
                return min;
            }
            else
            {
                while (firstTry || randNormal < min || randNormal > max)
                {
                    // Randomly draw a number based on desired normal distribution
                    u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
                    u2 = rand.NextDouble();
                    randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                    randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

                    // Keep trying until we fall within limits
                    firstTry = false;
                }
                return (float)randNormal;
            }
        }

        virtual public List<PrimitiveTriple<float, float, float>> Generate(GridMath gridMath, Random rand)
        {
            // List to return
            List<PrimitiveTriple<float, float, float>> posList = new List<PrimitiveTriple<float, float, float>>();

            // First determine the # of units
            int numUnits = (int)Math.Round(RandN(numMean, numStdDev, (float)numMin, (float)numMax, rand));

            // For each unit
            PrimitivePair<float, float> tempPos = new PrimitivePair<float, float>(0, 0);
            PrimitivePair<float, float> anchor;
            PrimitivePair<int, int> tempGrid;
            bool laydownFound;
            for (int i = 0; i < numUnits; i++)
            {
                // Laydown not valid by default
                laydownFound = false;

                // Keep trying points until we find one that satisfies grid constraints
                while (!laydownFound)
                {
                    // Randomly pick an anchor (all anchors already in world coordinates)
                    anchor = anchors[rand.Next(0, anchors.Count)]; // Note: rand.Next max value is exclusive

                    // Now independently pick X and Z deviations from that point
                    tempPos.first = anchor.first + RandN(0.0f, fromAnchorStdDev_km, 0.0f, fromAnchorMax_km, rand);
                    tempPos.second = anchor.second + RandN(0.0f, fromAnchorStdDev_km, 0.0f, fromAnchorMax_km, rand);

                    // Convert that temp position to grid
                    tempGrid = gridMath.WorldToGrid(tempPos);

                    // Check to see if the grid is within allowed
                    if (allowedCells.Contains(tempGrid))
                    {
                        laydownFound = true;
                    }
                }

                // Save the 3D point
                posList.Add(new PrimitiveTriple<float, float, float>(tempPos.first, 0, tempPos.second));
            }

            // Return list of randomized positions
            return posList;
        }
    }
}
