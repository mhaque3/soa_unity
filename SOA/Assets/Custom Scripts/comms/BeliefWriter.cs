using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace soa
{
    class BeliefWriter : BeliefQueueWorker
    {
        private AbstractNetwork network;
        private Serializer serializer;
     
        public BeliefWriter(Serializer serializer, AbstractNetwork network)
            : base()
        {
            this.network = network;
            this.serializer = serializer;
        }
        
        public void write(Belief belief)
        {
            addBelief(belief);
        }

        override
        protected void run()
        {
            while (IsAlive())
            {
                Belief belief = popBelief();
                if (belief != null)
                {
                    byte[] messageData = serializer.serializeBelief(belief);
                    if (messageData != null)
                    {
                        network.Send(messageData, messageData.Length);
                    }
                }
            }
        }
    }
}
