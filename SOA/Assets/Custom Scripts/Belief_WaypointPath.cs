using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Waypoint
    {
        public float x, y, z, heading;
        public bool visited;
    }

	public class Belief_WaypointPath : Belief
	{

        private List<Waypoint> waypoints;
        private ulong requestTime;

        public Belief_WaypointPath(ulong requestTime, int id, List<Waypoint> path)
            : base(id)
        {
            this.requestTime = requestTime;
            this.waypoints = new List<Waypoint>(path); 
        }

        public override BeliefType getBeliefType()
        {
            return BeliefType.WAYPOINT_PATH;
        }

        public List<Waypoint> getWaypoints()
        {
            return new List<Waypoint>(waypoints);
        }

        public ulong getRequestTime()
        {
            return requestTime;
        }
	}
}
