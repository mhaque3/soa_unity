using System;
using System.Linq;
using System.Text;

namespace soa
{
    public class GridMath
    {
        // Unit basis vector (in world coordinates) defining hex grid u-axis direction
        // (Hard coded for now since grid type/axes are fixed)
        float uHat_x = (float)Math.Sqrt(3.0f) / 2.0f;
        const float uHat_z = 1.0f / 2.0f;

        // Unit basis vector (in world coordinates) defining hex grid v-axis direction
        // (Hard coded for now since grid type/axes are fixed)
        const float vHat_x = 0.0f;
        const float vHat_z = 1.0f;

        // Private members
        private float gridOrigin_x;
        private float gridOrigin_z;
        private float gridToWorldScale;

	    // Constructor used to set scaling and offset values
	    // gridOrigin_x - The x-component of grid coordinate origin (u=0, v=0) as expressed in world (x,z) coordinates
	    // gridOrigin_z - The z-component of grid coordinate origin (u=0, v=0) as expressed in world (x,z) coordinates
	    // gridToWorldScale - Length of a line (in world coordinates) that connects the centers of two adjacent hex cells
        public GridMath(float gridOrigin_x, float gridOrigin_z, float gridToWorldScale)
        {
            this.gridOrigin_x = gridOrigin_x;
            this.gridOrigin_z = gridOrigin_z;
            this.gridToWorldScale = gridToWorldScale;
        }

        // Transforms a hex cell center (u,v) in grid coordinates to a point (x,z) in world coordinates
	    // Based on reference: http://www.redblobgames.com/grids/hexagons/#hex-to-pixel
	    public PrimitivePair<float,float> GridToWorld(PrimitivePair<int,int> gridCoordinate)
        {
            // Extract inputs
            int grid_u = gridCoordinate.first;
            int grid_v = gridCoordinate.second;

		    // World x-coordinate
		    float world_x = gridToWorldScale*(uHat_x*((float)grid_u) + vHat_x*((float)grid_v)) + gridOrigin_x;

		    // World z-coordinate
		    float world_z = gridToWorldScale*(uHat_z*((float)grid_u) + vHat_z*((float)grid_v)) + gridOrigin_z;

            // Package and return to caller
            return new PrimitivePair<float, float>(world_x, world_z);
        }

        // Transforms a point (x,z) in world coordinates to the (u,v) of the hex cell that contains it
	    // Based on references: http://www.redblobgames.com/grids/hexagons/#pixel-to-hex
	    //                      http://www.redblobgames.com/grids/hexagons/#rounding
	    public PrimitivePair<int,int> WorldToGrid(PrimitivePair<float,float> worldCoordinate)
        {
            // Extract inputs
            float world_x = worldCoordinate.first;
            float world_z = worldCoordinate.second;

		    // Compute determinant of transform matrix for inverse
		    float det = uHat_x*vHat_z - vHat_x*uHat_z;
		
		    // Compute offsets from grid origin
		    float rel_x = world_x - gridOrigin_x;
		    float rel_z = world_z - gridOrigin_z;

		    // Cube u-coordinate
		    float cube_u = (vHat_z*rel_x - vHat_x*rel_z)/(gridToWorldScale*det);

    		// Cube v-coordinate
	    	float cube_v = (-uHat_z*rel_x + uHat_x*rel_z)/(gridToWorldScale*det);

		    // Cube w-coordinate
		    float cube_w = -cube_u - cube_v;
            
		    // Round cube coordinates to nearest integer
            float round_u = (float) Math.Round(cube_u); 
		    float round_v = (float) Math.Round(cube_v);
		    float round_w = (float) Math.Round(cube_w);

		    // Compute residual magnitudes
		    float diff_u = Math.Abs(round_u - cube_u);
		    float diff_v = Math.Abs(round_v - cube_v);
		    float diff_w = Math.Abs(round_w - cube_w);

	    	// Snap to nearest hex center
	    	if(diff_u > diff_v && diff_u > diff_w)
            {
	    		round_u = -round_v - round_w;
	    	}else if(diff_v > diff_w)
            {
	    		round_v = -round_u - round_w;
	    	}

	    	// Cast to int to return to caller
            return new PrimitivePair<int, int>((int)round_u, (int)round_v);
	    }
    }
}

