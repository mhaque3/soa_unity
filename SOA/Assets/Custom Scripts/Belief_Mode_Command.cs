using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Mode_Command : Belief
    {
        // Members
        private ulong request_time;
        private int actor_id;
        private int mode_id;

        // Constructor
        public Belief_Mode_Command(ulong request_time, int actor_id, int mode_id)
            : base(actor_id)
        {
            this.request_time = request_time;
            this.actor_id = actor_id;
            this.mode_id = mode_id;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.MODE_COMMAND;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Mode_Command {"
                + "\n" + "  request_time: " + request_time
                + "\n" + "  actor_id: " + actor_id
                + "\n" + "  mode_id: " + mode_id
                + "\n" + "}";
            return s;
        }

        // Get methods
	    public ulong getRequest_time(){ return request_time; }
	    public int getActor_id(){ return actor_id; }
	    public int getMode_id(){ return mode_id; }
    }
}
