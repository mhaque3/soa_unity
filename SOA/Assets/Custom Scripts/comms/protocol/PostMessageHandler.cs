using System;
namespace soa
{
	public class PostMessageHandler
	{
		private readonly BeliefRepository repo;
		private readonly AgentMessageHandler protocol;
		private readonly Serializer serializer;

		public PostMessageHandler(BeliefRepository repo, AgentMessageHandler protocol, Serializer serializer)
		{
			this.repo = repo;
			this.protocol = protocol;
			this.serializer = serializer;
		}

		public void handleMessage(BSPMessage message)
		{
			Belief belief = serializer.generateBelief(message.getData().getBuffer());
            if (belief.getBeliefType() == Belief.BeliefType.WAYPOINT_PATH)
            {
                Log.debug("Received waypoint path");
            }
			repo.Commit(belief);
		}

		public void post(CachedBelief belief)
		{
            if (protocol.getConnection().getRemoteAddress() == null)
            {
                return;
            }
            
			byte[] bufferData = serializer.serializeBelief(belief.GetBelief());
			BSPMessage message = new BSPMessage(protocol.getConnection().getRemoteAddress(),
			                                    BSPMessageType.POST,
			                                    protocol.getAgentID(),
			                                    new NetworkBuffer(bufferData));
			protocol.getConnection().send(message);
		}
	}
}

