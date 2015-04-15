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
            BASE, 
            GRIDSPEC,
            MODE_COMMAND,
            NGOSITE, 
            ROADCELL, 
            SPOI, 
            TERRAIN,
            TIME,
            VILLAGE, 
            WAYPOINT, 
            WAYPOINT_OVERRIDE
        }; 

        // Each belief must be able to give its type
        abstract public BeliefType getBeliefType();
    }
}
