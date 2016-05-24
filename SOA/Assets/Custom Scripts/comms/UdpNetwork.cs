using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace soa
{
    class UdpNetwork : AbstractNetwork
    {
        private Serializer serializer;
        private UdpClient socket;
        private IPEndPoint server;
        private bool connected;

        public UdpNetwork(Serializer serializer, IPEndPoint server)
        {
            this.serializer = serializer;
            this.server = server;
            this.socket = new UdpClient();

            try
            {
                socket.Connect(server);
                connected = true;
            } catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                connected = false;
            }
        }

        public void connect(IPEndPoint server)
        {
            try
            {
                this.socket.Connect(server);
                this.server = server;
                this.connected = true;
            }
            catch (SocketException exp)
            {
                Console.Error.WriteLine(exp.ToString());
                this.connected = false;
            }
        }

        public override byte[] Receive()
        {
            try
            {
                return socket.Receive(ref server);
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return null;
            }
        }

        public override void Send(byte[] buffer, int lenght)
        {
            try
            {
                socket.Send(buffer, buffer.Length);
            } 
            catch(Exception exp)
            {
                Console.Error.WriteLine(exp.ToString());
            }
        }
    }
}
