using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief
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

        private static System.DateTime epoch = new System.DateTime(1970, 1, 1);

        public Belief(int id)
        {
            this.id = id;
            time = (UInt64)(System.DateTime.UtcNow - epoch).Milliseconds;
        }

        // Each belief must be able to give its type
        public virtual BeliefType getBeliefType()
        {
            return BeliefType.INVALID;
        }


        public int getId()
        {
            return id;
        }
        public UInt64 getTime()
        {
            return time;
        }

        private UInt64 time;
        private int id;

    }
}
