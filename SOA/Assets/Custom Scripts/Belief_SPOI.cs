using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_SPOI : Belief
    {
        // Members
        private ulong request_time;
        private int actor_id;
        private float pos_x;
        private float pos_z;

        // Constructor
        public Belief_SPOI(ulong request_time, int actor_id, float pos_x, float pos_z)
            : base(actor_id)
        {
            this.request_time = request_time;
            this.actor_id = actor_id;
            this.pos_x = pos_x;
            this.pos_z = pos_z;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.SPOI;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_SPOI {"
                + "\n" + "  request_time: " + request_time
                + "\n" + "  actor_id: " + actor_id
                + "\n" + "  pos_x: " + pos_x
                + "\n" + "  pos_z: " + pos_z
                + "\n" + "}";
            return s;
        }

        // Get methods
        public ulong getRequest_time() { return request_time; }
        public int getActor_id() { return actor_id; }
        public float getPos_x() { return pos_x; }
        public float getPos_z() { return pos_z; }
    }
}
