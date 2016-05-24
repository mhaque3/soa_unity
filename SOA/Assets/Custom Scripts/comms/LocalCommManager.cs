using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace soa
{
    public class LocalCommManager
    {
        private DataManager dataManager;
        private Dictionary<int, ClientConnection> sockets;
        private AbstractNetwork discoveryPort;
        private Thread connectionThread;
        private bool alive;

        public LocalCommManager(DataManager dataManager, AbstractNetwork network) : base()
        {
            this.dataManager = dataManager;
            this.sockets = new Dictionary<int, ClientConnection>();
            this.discoveryPort = network;
        }

        public void Start()
        {
            this.alive = true;
            this.connectionThread = new Thread(handleConnections);
            connectionThread.Name = "Connection Thread";
            connectionThread.Start();
        }

        public void Stop()
        {
            alive = false;
            connectionThread.Join();
        }

        private void handleConnections()
        {
            while(alive)
            {
                byte[] messageData = discoveryPort.Receive();
                string messageText = System.Text.Encoding.Default.GetString(messageData);
            }
        }
    }
}

