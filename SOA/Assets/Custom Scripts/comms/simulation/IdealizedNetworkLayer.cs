using System.Collections.Generic;

namespace soa
{
    class IdealizedNetworkLayer : IPhysicalLayer<int>
    {
        private ConnectivityGraph connectivity;
        private Dictionary<int, AgentCommunicator> communicators;

		public IdealizedNetworkLayer(IWorld world)
        {
			connectivity = new ConnectivityGraph(world);
            communicators = new Dictionary<int, AgentCommunicator>();
        }

        public Communicator<int> BuildCommunicatorFor(int actorID)
        {
            return GetCommsInternal(actorID);
        }

        public void Update()
        {
            connectivity.UpdateConnectivity();
        }

        private IEnumerable<AgentCommunicator> GetCommunicatorsInRange(int actorID, int filterID=-1)
        {
            List<AgentCommunicator> comms = new List<AgentCommunicator>();
            foreach (ISoaActor actor in connectivity.GetActorsInCliqueOf(actorID))
            {
				AgentCommunicator neighborComm = GetCommsInternal(actor.getID());
                if (neighborComm.callback != null && filterID == -1 || filterID == neighborComm.actorID)
                {
                    comms.Add(neighborComm);
                }
            }
            return comms;
        }

        private AgentCommunicator GetCommsInternal(int actorID)
        {
            AgentCommunicator comms = null;
            if (!communicators.TryGetValue(actorID, out comms))
            {
                comms = new AgentCommunicator(this, actorID);
            }
            
            return comms;
        }

        private class AgentCommunicator : Communicator<int>
        {
            public int actorID;
            private IdealizedNetworkLayer parent;
            public  CommunicatorCallback<int> callback;

            public AgentCommunicator(IdealizedNetworkLayer parent, int actorID)
            {
                this.parent = parent;
                this.actorID = actorID;
            }

            public int GetBandwidth()
            {
                return int.MaxValue;
            }

            public void RegisterCallback(CommunicatorCallback<int> callback)
            {
                this.callback = callback;
            }

            public void Send(Belief belief, int address)
            {
				sendToAll(belief, actorID, parent.GetCommunicatorsInRange(actorID, address));
            }

			public void Broadcast(Belief belief)
            {
                sendToAll(belief, actorID, parent.GetCommunicatorsInRange(actorID));
            }

			private void sendToAll(Belief belief, int sourceID, IEnumerable<AgentCommunicator> comms)
            {
                foreach (AgentCommunicator comm in comms)
                {
                    comm.callback(belief, sourceID);
                }
            }
        }
    }
}
