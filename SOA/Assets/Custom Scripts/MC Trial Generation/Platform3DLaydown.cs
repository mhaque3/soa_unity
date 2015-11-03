using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Platform3DLaydown : Platform2DLaydown
    {
        public float altitudeMean_km;
        public float altitudeStdDev_km;
        public float altitudeMin_km;
        public float altitudeMax_km;

        public Platform3DLaydown() { }

        public override List<PrimitiveTriple<float, float, float>> Generate(GridMath gridMath, Random rand)
        {
            // Call base function to generate 2D positions
            List<PrimitiveTriple<float,float,float>> posList = base.Generate(gridMath, rand);

            // Set altitudes randomly
            foreach(PrimitiveTriple<float,float,float> tempPos in posList)
            {
                tempPos.second = RandN(altitudeMean_km, altitudeStdDev_km, altitudeMin_km, altitudeMax_km, rand);
            }

            return posList;
        }
    }
}
