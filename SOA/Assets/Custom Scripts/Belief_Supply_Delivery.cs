using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Supply_Delivery : Belief
    {
        // Members
        private ulong request_time;
        private int actor_id;
        private bool deliver_anywhere;
        private int[] destination_ids;

        // Constructor
        public Belief_Supply_Delivery(ulong request_time, int actor_id, bool deliver_anywhere, int[] destination_ids)
            : base(actor_id)
        {
            this.request_time = request_time;
            this.actor_id = actor_id;
            this.deliver_anywhere = deliver_anyhwere;
            this.destination_ids = new int[destination_ids.Length];
            Array.Copy(destination_ids, this.destination_ids, destination_ids.Length);
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.SUPPLY_DELIVERY;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Supply_Delivery {"
                + "\n" + "  request_time: " + request_time
                + "\n" + "  actor_id: " + actor_id
                + "\n" + "  deliver_anywhere: " + deliver_anywhere
                + "\n" + "  destination_ids: [";
            for (int i = 0; i < destination_ids.Length; i++)
            {
                s += destination_ids[i];
                if(i < destination_ids.Length - 1)
                {
                    s += ", ";
                }
            }
            s += "]\n}";
            return s;
        }

        // Get methods
        public ulong getRequest_time() { return request_time; }
        public int getActor_id() { return actor_id; }
        public bool getDeliver_anywhere() { return deliver_anywhere; }
        public int[] getDestination_ids()
        {
            // Return a copy, not reference to internal data
            int[] return_ids = new int[destination_ids.Length];
            Array.Copy(destination_ids, return_ids, destination_ids.Length);
            return return_ids;
        }
    }
}
