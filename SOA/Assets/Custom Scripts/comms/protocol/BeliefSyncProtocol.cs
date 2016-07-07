using System;
using System.Collections.Generic;
using System.Net;

namespace soa
{
	public class BeliefSyncProtocol
	{
		private Dictionary<int, AgentMessageHandler> handlers;
		private IMessageWriter messageWriter;

		public BeliefSyncProtocol(IMessageWriter messageWriter)
		{
			this.handlers = new Dictionary<int, AgentMessageHandler>();
			this.messageWriter = messageWriter;
		}

		public void synchronizeAllBeliefsFor(int actorID)
		{
			AgentMessageHandler handler = findHandler(actorID);
			if (handler != null)
			{
				handler.synchronizeAllBeliefs();
			}
		}

		public void synchronizeBelief(Belief belief, int sourceID)
		{
			AgentMessageHandler handler = findHandler(actorID);
			if (handler != null)
			{
				handler.synchronizeBelief(Belief);
			}
		}

		public string getConnectionString(int actorID)
		{
			AgentMessageHandler handler = findHandler(actorID);
			if (handler != null)
			{
				IPEndPoint address = handler.getConnection().getAddress();
				if (address != null)
				{
					return address.ToString();
				}
			}
			return "Not Connected";
		}

		public void addActor(SoaActor actor)
		{
			INetworkConnection connection = new AgentConnection(this);
			handlers.Item[actor.unique_id] = new AgentMessageHandler(actor.getRepository(), connection);
		}

		public void handleMessage(Message message)
		{
			BSPMessage bspMessage = parse(message);
			AgentMessageHandler handler = null;
			if (handler.TryGetValue(bspMessage.getSourceID(), out handler))
			{
				handler.handleMessage(bspMessage);
			}
		}

		private AgentMessageHandler findHandler(int actorID)
		{
			AgentMessageHandler handler = null;
			handlers.TryGetValue(actorID, out handler);
			return handler;
		}

		private Message formatMessage (BSPMessage data)
		{
			if (data == null) {
				Console.Error.WriteLine ("Could not format null data");
				return null;
			}

			int messageLength = data.messageData == null ? 0 : data.messageData.Length;
			NetworkBuffer messageData = new NetworkBuffer(HEADER_LENGTH + messageLength);
			messageData.writeInt32 (HEADER_TYPE_OFFSET, (int)data.type);
			messageData.writeInt32 (HEADER_SOURCE_OFFSET, data.sourceID);

			if (data.messageData != null) {
				messageData.writeBytes(data.getData().getBuffer(), 0, HEADER_LENGTH, data.getData().size());
			}
		
            return new Message(data.address, messageData);
        }

        private BSPMessage parse(Message message)
        {
			if (message == null) {
				Console.Error.WriteLine ("Could not parse null message");
				return null;
			}

            if (message.data.Length < HEADER_LENGTH) {
                throw new Exception("Invalid message: " + System.Text.Encoding.Default.GetString(message.data));
            }

            int messageType = parseInt32(message.data, HEADER_TYPE_OFFSET);
			BSPMessageType type = BSPMessageType.UNKNOWN;
            if (Enum.IsDefined(typeof(BSPMessageType), messageType)) {
                type = (BSPMessageType)messageType;
            }

            int sourceID = parseInt32(message.data, HEADER_SOURCE_OFFSET);

            int bytesRemaining = message.data.Length - HEADER_LENGTH;
			NetworkBuffer data = new NetworkBuffer(bytesRemaining);

            if (bytesRemaining > 0) {   
				data.writeBytes(message.data, HEADER_LENGTH, 0, bytesRemaining);
            }

			return new BSPMessage(message.address, type, sourceID, data);
        }

		private class AgentConnection : INetworkConnection
		{
			private readonly BeliefSyncProtocol protocol;
			private IPEndPoint agentAddress;

			public AgentConnection(BeliefSyncProtocol protocol)
			{
				this.protocol = protocol;
			}

			public void setRemoteAddress(IPEndPoint address)
			{
				this.agentAddress = address;
			}

			public IPEndPoint getRemoteAddress()
			{
				return agentAddress;
			}

			public void send(BSPMessage bspMessage)
			{
				Message message = protocol.format(bspMessage);
				protocol.messageWriter.write(message);
			}
		}
	}
}

