using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    abstract public class Belief
    {
        public enum BeliefType
        {
            INVALID = 0,
            ACTOR, 
            BASECELL, 
            MODE_COMMAND,
            NGOSITECELL, 
            NOGO, 
            ROAD, 
            SPOI, 
            TIME,
            VILLAGECELL, 
            WAYPOINT, 
            WAYPOINT_OVERRIDE
        }; 

        // Each belief must be able to give its type
        abstract public BeliefType getBeliefType();
    }
}
