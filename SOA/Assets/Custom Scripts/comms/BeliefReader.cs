using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace soa
{
    class BeliefReader : BeliefQueueWorker
    {
        private static int MAX_MESSAGE_SIZE = 65500;

        private AbstractNetwork socket;
        private Serializer serializer;
       
        public BeliefReader(Serializer serializer, AbstractNetwork socket)
            : base()
        {
            this.serializer = serializer;
            this.socket = socket;
        }

        override
        protected void run()
        {
            while (IsAlive())
            {

                byte[] messageData = socket.Receive();
                if (messageData != null)
                {
                    Belief belief = serializer.generateBelief(messageData);
                    if (belief != null)
                    {
                        addBelief(belief);
                    }
                }
            }
        }
    }
}
