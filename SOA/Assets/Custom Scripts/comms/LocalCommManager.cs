using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace soa
{
    public class LocalCommManager : ICommManager
    {
        private IDataManager dataManager;
        private Dictionary<int, IPEndPoint> agentIPs;
        private INetwork network;
        private readonly MessageReader reader;
        private readonly MessageWriter writer;
        private readonly Serializer serializer;
        private ConnectionProtocol protocol;

        public LocalCommManager(IDataManager dataManager, Serializer serializer, INetwork network)
        {
            this.dataManager = dataManager;
            this.network = network;
            this.serializer = serializer;
            this.agentIPs = new Dictionary<int, IPEndPoint>();
            this.writer = new MessageWriter(network);
            this.reader = new MessageReader(handleRequest, network);
            this.protocol = new ConnectionProtocol();
        }

        public void start()
        {
            reader.Start();
            writer.Start();
        }

        public void terminate()
        {
            reader.Stop();
            writer.Stop();
        }

        public void addOutgoing(List<Belief> beliefs, int sourceID, params int[] targetIDs)
        {
            foreach(Belief belief in beliefs)
            {
                addOutgoing(belief, sourceID, targetIDs);
            }
        }

        public void addOutgoing(Belief b, int sourceID, int[] targetActorIDs)
        {
            foreach (int agentID in targetActorIDs)
            {
                ConnectionProtocol.RequestData msgData = new ConnectionProtocol.RequestData();
                IPEndPoint address = null;
                if (agentIPs.TryGetValue(agentID, out address))
                {
                    msgData.address = address;
                    msgData.sourceID = sourceID;
                    msgData.type = ConnectionProtocol.RequestType.POST;
                    msgData.messageData = serializer.serializeBelief(b);

                    Message message = protocol.formatMessage(msgData);
                    writer.write(message);
                }
            }
        }

        private void handleRequest(ConnectionProtocol.RequestData request)
        {
            switch (request.type)
            {
                case ConnectionProtocol.RequestType.CONNECT:
                    handleConnection(request);
                    break;
                case ConnectionProtocol.RequestType.POST:
                    handlePost(request);
                    break;
            }
        }

        private void handleConnection(ConnectionProtocol.RequestData connectionRequest)
        {
            agentIPs.Add(connectionRequest.sourceID, connectionRequest.address);

            List<Belief> initializationBeliefs = dataManager.getInitializationBeliefs();
            addOutgoing(initializationBeliefs, ConnectionProtocol.SERVER_ID,connectionRequest.sourceID);
        }

        private void handlePost(ConnectionProtocol.RequestData postRequest)
        {
            Belief belief = serializer.generateBelief(postRequest.messageData);
            dataManager.addExternalBeliefToActor(belief, postRequest.sourceID);
        }
    }
}

