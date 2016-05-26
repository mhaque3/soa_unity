using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace soa
{
    class UdpNetwork : INetwork
    {
        private UdpClient socket;

        public UdpNetwork()
        {
            this.socket = new UdpClient();
        }

        public void Start()
        {}

        public void Stop()
        {
            this.socket.Close();
        }

        public Message Receive()
        {
            try
            {
                IPEndPoint connectionAddress = new IPEndPoint(IPAddress.Any, 8080);
                byte[] messageData = socket.Receive(ref connectionAddress);

                if (messageData == null || messageData.Length == 0)
                    return null;

                return new Message(connectionAddress, messageData);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return null;
            }
        }

        public void Send(Message message)
        {
            try
            {
                socket.Send(message.data, message.data.Length, message.address);
            } 
            catch(Exception exp)
            {
                Console.Error.WriteLine(exp.ToString());
            }
        }
    }
}
