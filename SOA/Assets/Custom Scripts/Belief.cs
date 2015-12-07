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
            CASUALTY_DELIVERY,
            CASUALTY_PICKUP,
            GRIDSPEC,
            MODE_COMMAND,
            NGOSITE, 
            ROADCELL, 
            SPOI, 
            SUPPLY_DELIVERY,
            SUPPLY_PICKUP,
            TERRAIN,
            TIME,
            VILLAGE, 
            WAYPOINT, 
            WAYPOINT_OVERRIDE,
            CUSTOM
        };

        private static System.DateTime epoch = new System.DateTime(1970, 1, 1);

        public Belief(int id)
        {
            this.id = id;
            beliefTime = (UInt64)(System.DateTime.UtcNow - epoch).Ticks/10000;
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
        public UInt64 getBeliefTime()
        {
            return beliefTime;
        }

        public void setBeliefTime(UInt64 beliefTime)
        {
            this.beliefTime = beliefTime;
        }

        private UInt64 beliefTime;
        private int id;
    }
}
