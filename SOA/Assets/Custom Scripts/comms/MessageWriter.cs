using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace soa
{
    public class MessageWriter : ThreadWorker
    {
        private const int MSG_QUEUE_MAX_SIZE = 1024;
        private readonly BlockingCollection<Message> messages;
        private readonly INetwork network;

        public MessageWriter(INetwork network)
        {
            this.network = network;
            this.messages = new BlockingCollection<Message>(MSG_QUEUE_MAX_SIZE);
        }

        public void write(Message message)
        {
            messages.Add(message);
        }

        protected override void doWork()
        {
            while (isAlive())
            {
                Message message = messages.Take(cancelled.Token);
                network.Send(message);
            }
        }
    }
}
