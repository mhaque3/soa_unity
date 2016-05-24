using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace soa
{
    class ClientConnection
    {
        private BeliefWriter writer;
        private BeliefReader reader;

        public ClientConnection(Serializer serializer, AbstractNetwork network)
        {
            this.writer = new BeliefWriter(serializer, network);
            this.reader = new BeliefReader(serializer, network);
        }

        public void start()
        {
            writer.Start();
            reader.Start();
        }

        public void stop()
        {
            writer.Stop();
            reader.Stop();
        }

        public void addBelief(Belief belief)
        {
            writer.write(belief);
        }
    }
}
