using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace soa
{
    public class MessageReader : ThreadWorker
    {
        public delegate void Callback(ConnectionProtocol.RequestData data);

        private readonly Callback callback;
        private readonly INetwork network;
        private readonly ConnectionProtocol protocol;

        public MessageReader(Callback callback, INetwork network)
        {
            this.callback = callback;
            this.network = network;
            this.protocol = new ConnectionProtocol();
        }

        protected override void doWork()
        {
            while (isAlive())
            {
                try
                {
                    readNextMessage();
                }
                catch (Exception e)
                {
                    Log.error(e.ToString());
                }
            }
        }

        private void readNextMessage()
        {
            Message message = network.Receive();
            if (message != null)
            {
                ConnectionProtocol.RequestData data = protocol.parse(message);
                if (isValid(data))
                {
                    callback(data);
                }
                else
                {
                    Log.warning("Received invalid message");
                }
            }
        }

        private bool isValid(ConnectionProtocol.RequestData data)
        {
            if (data == null)
                return false;

            return data.type != ConnectionProtocol.RequestType.UNKNOWN && data.sourceID >= 0;
        }
    }
}
