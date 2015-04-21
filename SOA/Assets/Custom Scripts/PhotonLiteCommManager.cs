// Additinonal using statements are needed if we are running in Unity
#if(NOT_UNITY)
#else
    using UnityEngine; // Uncomment this if running in Unity
    using Hashtable = ExitGames.Client.Photon.Hashtable; // Uncomment this if running in Unity#endif
#endif

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;

namespace soa
{
    class PhotonLiteCommManager : IPhotonPeerListener
    {
        // Reference to data manager
        private DataManager dataManager;

        // Reference to serializer
        private Serializer serializer;

        // Connection information
        private string ipAddress;
        private string roomName;

        // Sleep time between Photon server updates
        private int updateSleepTime_ms;

        // Default source ID to send along with Belief to data manager
        private int defaultSourceID;

        // Outgoing messages
        private Queue<Byte[]> outgoingQueue;
        private Queue<Byte[]> localQueue;

        // Settings for managing outgoing queue
        int maxQueueSize;
        bool overwriteWhenQueueFull;

        // Photon
        private PhotonPeer peer;
        private Thread photonUpdateThread;
        private int mActorNumber;
        private enum OpCodeEnum : byte
        {
            Join = 255,
            Leave = 254,
            RaiseEvent = 253,
            SetProperties = 252,
            GetProperties = 251
        }

        // Internal state
        public enum State
        {
            INITIALIZED = 0,
            CONNECTING,
            CONNECTED,
            JOINING,
            JOINED,
            LEAVING,
            DISCONNECTING,
            DISCONNECTED,
            TERMINATED
        };
        private State currState, prevState;
        private bool terminateRequested, startEligible;
        private System.Object startEligibleMutex;

        // Constructor
        public PhotonLiteCommManager(DataManager dataManager, Serializer serializer,
            string ipAddress, string roomName, int defaultSourceID, int updateSleepTime_ms = 10,
            int maxQueueSize = 1000000, bool overwriteWhenQueueFull = true)
        {
            // Copy variables
            this.dataManager = dataManager;
            this.serializer = serializer;
            this.ipAddress = ipAddress;
            this.roomName = roomName;
            this.defaultSourceID = defaultSourceID;
            this.updateSleepTime_ms = updateSleepTime_ms;
            this.maxQueueSize = maxQueueSize;
            this.overwriteWhenQueueFull = overwriteWhenQueueFull;

            // Initialize mutex
            startEligibleMutex = new System.Object();

            // Initialize queue
            outgoingQueue = new Queue<Byte[]>();
            localQueue = new Queue<Byte[]>();

            // Initialize photon listener peer
            peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            peer.DebugOut = DebugLevel.INFO;

            // Initialize internal state
            prevState = State.TERMINATED;
            currState = State.TERMINATED;

            // Used to keep track of whether a new thread can start
            startEligible = true;

            // Used to keep track of whether termination request has been sent
            terminateRequested = false;

            // Initialize thread references
            photonUpdateThread = null;
        }

        // Starts the communication manager
        public void start()
        {
            // Acquire lock
            lock (startEligibleMutex)
            {
                // Start photon update thread if not already running
                if (startEligible)
                {
                    // Cannot start another one
                    startEligible = false;

                    // Do not terminate the fsm
                    terminateRequested = false;

                    // Initialize internal state
                    currState = State.DISCONNECTED;

                    // Create and start new thread
                    photonUpdateThread = new Thread(new ThreadStart(photonUpdate));
                    photonUpdateThread.Start();

                    // Wait for it to start up
                    while (!photonUpdateThread.IsAlive) ;
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: Update thread started");                        
                    #else
                    Debug.Log("PhotonLiteCommManager: Update thread started");
                    #endif
                }
                else
                {
                    // Photon update thread already running, no need to do anything else
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: start() has no effect, update thread already running");                       
                    #else
                    Debug.Log("PhotonLiteCommManager: start() has no effect, update thread already running");
                    #endif
                }
            } // Unlock
        }

        // Shuts down the communication manager
        public void terminate()
        {
            // Acquire lock
            lock (startEligibleMutex)
            {
                // Stop photon thread if currently running
                if (!startEligible)
                {
                    // Request to disconnect has been sent
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: Termination request sent");                    
                    #else
                    Debug.Log("PhotonLiteCommManager: Termination request sent");
                    #endif
                    terminateRequested = true;

                    // Wait for termination
                    photonUpdateThread.Join();

                    // Since thread has terminated, can now start another
                    startEligible = true;
                }
                else
                {
                    // Photon update thread already inactive, no need to do anything else
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: terminate() has no effect, update thread already inactive");
                    #else
                    Debug.Log("PhotonLiteCommManager: terminate() has no effect, update thread already inactive");
                    #endif
                }
            } // Unlock
        }

        // Gets current state of communication manager
        public State getState()
        {
            return currState;
        }

        // Thread method that implements a finite state machine to connect to photon server and send outgoing messages periodically
        private void photonUpdate()
        {
            // Keep looping until conditions are met to terminate and exit fsm
            while (currState != State.TERMINATED)
            {
                // Print out if any state transitions have been made
                if (prevState != currState)
                {
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: " + getStateString(prevState) + " -> " + getStateString(currState));
                    #else
                    Debug.Log("PhotonLiteCommManager: " + getStateString(prevState) + " -> " + getStateString(currState));
                    #endif
                }

                // Make copy of current state
                prevState = currState;

                // Take action based on current state and whether we are in normal
                // operation mode or trying to terminate
                switch (currState)
                {
                    case State.DISCONNECTED: // No connections exist and no pending connections
                        {
                            if (terminateRequested)
                            {
                                // Indicate to the fsm to stop and exit
                                currState = State.TERMINATED;
                            }
                            else
                            {
                                // Try (re)connecting to the photon server
                                currState = State.CONNECTING;
                                #if(NOT_UNITY)
                                Console.WriteLine("PhotonLiteCommManager: Connecting to Photon server at " + ipAddress);                               
                                #else
                                Debug.Log("PhotonLiteCommManager: Connecting to Photon server at " + ipAddress);
                                #endif
                                peer.Connect(ipAddress, roomName);
                            }
                            break;
                        }
                    case State.CONNECTING: // Current connecting to server
                        {
                            if (terminateRequested)
                            {
                                // Cancel request to connect to server
                                currState = State.DISCONNECTING;
                                peer.Disconnect();
                            }
                            else
                            {
                                // Do nothing, just wait for response
                            }
                            break;
                        }
                    case State.CONNECTED: // Connected to the server, no request to join room sent yet
                        {
                            if (terminateRequested)
                            {
                                // Disconnect from the server
                                currState = State.DISCONNECTING;
                                peer.Disconnect();
                            }
                            else
                            {
                                // Connected to Photon server, now join our game
                                #if(NOT_UNITY)
                                Console.WriteLine("PhotonLiteCommManager: Joining room \"" + roomName + "\"");                                
                                #else
                                Debug.Log("PhotonLiteCommManager: Joining room \"" + roomName + "\"");
                                #endif
                                currState = State.JOINING;
                                Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                                opParams[LiteOpKey.GameId] = roomName;
                                peer.OpCustom((byte)LiteOpCode.Join, opParams, true);
                            }
                            break;
                        }
                    case State.JOINING: // Currently joining a room
                        {
                            if (terminateRequested)
                            {
                                // Cancel request to join room
                                currState = State.LEAVING;
                                Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                                opParams[LiteOpKey.GameId] = roomName;
                                peer.OpCustom((byte)LiteOpCode.Leave, opParams, true);
                            }
                            else
                            {
                                // Do nothing, just wait for response
                            }
                            break;
                        }
                    case State.JOINED: // Connected to both server and game
                        {
                            if (terminateRequested)
                            {
                                // Leave the room first
                                currState = State.LEAVING;
                                Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                                opParams[LiteOpKey.GameId] = roomName;
                                peer.OpCustom((byte)LiteOpCode.Leave, opParams, true);
                            }
                            else
                            {
                                // We are within a game room.  Send any messages waiting
                                // on our outgoing queue
                                sendOutgoing();
                            }
                            break;
                        }
                    default:
                        break;
                }

                // Print out if any state transitions have been made
                if (prevState != currState)
                {
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: " + getStateString(prevState) + " -> " + getStateString(currState));
                    #else
                    Debug.Log("PhotonLiteCommManager: " + getStateString(prevState) + " -> " + getStateString(currState));
                    #endif
                }

                // Make copy of current state
                prevState = currState;

                // Communicate with Photon server
                peer.Service();

                // Wait a while before talking with server again
                System.Threading.Thread.Sleep(updateSleepTime_ms);
            }
        }

        // Callback to handle photon server connection status messages and update internal state accordingly
        public void OnStatusChanged(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.Connect:
                    // Just connected to server
                    currState = State.CONNECTED;
                    break;
                case StatusCode.Disconnect:
                    // Just disconnected from server
                    currState = State.DISCONNECTED;
                    break;
                default:
                    break;
            }
        }

        // Callback to handle photon game connection status messages and update internal state accordingly
        public void OnOperationResponse(OperationResponse operationResponse)
        {
            switch (operationResponse.OperationCode)
            {
                case LiteOpCode.Join:
                    // Just successfully joined a room
                    mActorNumber = (int)operationResponse.Parameters[LiteOpKey.ActorNr];
                    currState = State.JOINED;
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonLiteCommManager: Joined room \"" + roomName + "\" as Player " + mActorNumber);
                    #else
                    Debug.Log("PhotonLiteCommManager: Joined room \"" + roomName + "\" as Player " + mActorNumber);
                    #endif
                    break;
                case LiteOpCode.Leave:
                    // Just successfully left a room, now we are connected to server only
                    currState = State.CONNECTED;
                    break;
                default:
                    break;
            }
        }

        // Callback to handle messages received over Photon game network
        public void OnEvent(EventData eventData)
        {
            // Take action based on received event code
            switch (eventData.Code)
            {
                case 0: // Received a Protobuff message
                    {
                        // Get the byte[] message from protobuf
                        // Message = 1 byte for length of rest of message, 4 bytes for source ID, rest is serialized belief
                        Hashtable evData = (Hashtable)eventData.Parameters[LiteEventKey.Data];
                        Byte[] message = (Byte[])evData[(byte)0];

                        // Extract belief length and source ID
                        int serializedBeliefLength = (int)message[0] - 4;
                        int sourceID = BitConverter.ToInt32(message, 1); // get from bytes 1 through 4 in array

                        // Extract serialized belief
                        Byte[] serializedBelief = new Byte[serializedBeliefLength];
                        System.Buffer.BlockCopy(message, 5, serializedBelief, 0, serializedBeliefLength);

                        // Extract the belief
                        Belief b = serializer.generateBelief(serializedBelief);

                        // If deserialization was successful
                        if (b != null)
                        {
                            #if(NOT_UNITY)
                            // Filter the belief
                            if (dataManager.filterBelief(b))
                            {
                            #endif
                                // Add the belief to the data manager if it passed filter
                                dataManager.addBelief(b, sourceID);
                            #if(NOT_UNITY)
                            }
                            #endif
                        }
                        else
                        {
                            // Something went wrong with deserialization
                            #if(NOT_UNITY)
                            Console.Error.WriteLine("PhotonLiteCommManager: Belief deserialization failed");
                            #else
                            Debug.Log("PhotonLiteCommManager: Belief deserialization failed");
                            #endif
                        }
                        break;
                    }
                case LiteEventCode.Join:
                    {
                        // New player just joined, print status if that player isn't me
                        int actorNrJoined = (int)eventData.Parameters[LiteEventKey.ActorNr];
                        if (actorNrJoined != mActorNumber)
                        {
                            #if(NOT_UNITY)
                            Console.WriteLine("PhotonLiteCommManager: Player " + actorNrJoined + " joined the room");                            
                            #else
                            Debug.Log("PhotonLiteCommManager: Player " + actorNrJoined + " joined the room");
                            #endif
                        }

                        // Provide update on all players in room
                        int[] actorList = (int[])eventData.Parameters[LiteEventKey.ActorList];
                        if (actorList.Length > 0)
                        {
                            string statusMessage = "PhotonLiteCommManager: Players currently in room {" + actorList[0];
                            for (int i = 1; i < actorList.Length; i++)
                            {
                                statusMessage += (", " + actorList[i]);
                            }
                            statusMessage += "}";
                            #if(NOT_UNITY)
                            Console.WriteLine(statusMessage);                            
                            #else
                            Debug.Log(statusMessage);
                            #endif
                        }
                        break;
                    }
                case LiteEventCode.Leave:
                    {
                        // Exiting player just left, print status
                        int actorNrJoined = (int)eventData.Parameters[LiteEventKey.ActorNr];
                        #if(NOT_UNITY)
                        Console.WriteLine("PhotonLiteCommManager: Player " + actorNrJoined + " left the room");
                        #else
                        Debug.Log("PhotonLiteCommManager: Player " + actorNrJoined + " left the room");
                        #endif

                        // Provide update on all players in room
                        int[] actorList = (int[])eventData.Parameters[LiteEventKey.ActorList];
                        if (actorList.Length > 0)
                        {
                            string statusMessage = "PhotonLiteCommManager: Players currently in room {" + actorList[0];
                            for (int i = 1; i < actorList.Length; i++)
                            {
                                statusMessage += (", " + actorList[i]);
                            }
                            statusMessage += "}";
                            #if(NOT_UNITY)
                            Console.WriteLine(statusMessage);                            
                            #else
                            Debug.Log(statusMessage);
                            #endif
                        }
                        break;
                    }
                default:
                    // Ignore other events
                    break;
            }
        }

        /// <summary>
        /// Adds information from data manager to outgoing queue using default source ID
        /// </summary>
        public void addOutgoing(Belief b)
        {
            addOutgoing(b, defaultSourceID);
        }

        // Adds information from data manager to outgoing queue
        public void addOutgoing(Belief b, int sourceID)
        {
            // Serialize the source ID into 4 bytes, network byte order (Big Endian)
            Byte[] sourceIDBytes = BitConverter.GetBytes(sourceID);

            // Serialize the belief
            Byte[] beliefBytes = serializer.serializeBelief(b);

            // Combine the serialized source ID and belief into one message
            Byte[] message = new Byte[sourceIDBytes.Length + beliefBytes.Length];
            System.Buffer.BlockCopy(sourceIDBytes, 0, message, 0, sourceIDBytes.Length);
            System.Buffer.BlockCopy(beliefBytes, 0, message, sourceIDBytes.Length, beliefBytes.Length);

            // Enqueue the serialized message if serialization was
            // successful (if serial is nonempty)
            if (beliefBytes.Length > 0)
            {
                lock (outgoingQueue)
                {
                    if (outgoingQueue.Count < maxQueueSize)
                    {
                        // There is still space in queue, go ahead and enqueue
                        outgoingQueue.Enqueue(message);
                    }
                    else if (outgoingQueue.Count == maxQueueSize && overwriteWhenQueueFull)
                    {
                        // No more space left and our policy is to overwrite old data
                        // Dequeue the oldest entry before enqueuing the new data
                        outgoingQueue.Dequeue();
                        outgoingQueue.Enqueue(message);
                    }
                    else
                    {
                        // No space left, policy says to do nothing and drop newest entry
                    }
                } // Unlock
            }
            else
            {
                // Something went wrong with serialization
                #if(NOT_UNITY)
                Console.Error.WriteLine("PhotonLiteCommManager: Belief serialization failed");
                #else
                Debug.Log("PhotonLiteCommManager: Belief serialization failed");
                #endif
            }
        }

        // Sends any outgoing messages that are on the queue
        private void sendOutgoing()
        {
            // Protobuf message
            Byte[] serial;
            Byte[] message;

            // Acquire lock
            lock (outgoingQueue)
            {
                // Take everything from outgoing queue and put into local queue
                // to minimize time spent with lock (in case something causes
                // sending over Photon to hang
                while (outgoingQueue.Count > 0)
                {
                    // Pop out first element in queue
                    serial = outgoingQueue.Dequeue();

                    // Push into local queue
                    localQueue.Enqueue(serial);
                }
            } // Unlock

            // Now send out everything that is in the local queue
            while (localQueue.Count > 0)
            {
                // Pop out first element in queue
                serial = localQueue.Dequeue();

                // Create message by appending own size header
                message = new Byte[serial.Length + 1];
                message[0] = (byte)serial.Length;
                System.Buffer.BlockCopy(serial, 0, message, 1, serial.Length);

                // Send it out over Photon to everyone else
                Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                opParams[LiteOpKey.Code] = (byte)0;
                Hashtable evData = new Hashtable();
                evData[(byte)0] = message;
                opParams[LiteOpKey.Data] = evData;
                peer.OpCustom((byte)LiteOpCode.RaiseEvent, opParams, true);
            }
        }

        public void DebugReturn(DebugLevel level, string message)
        {
            #if(NOT_UNITY)
            //Console.WriteLine("PhotonLiteCommManager: DebugReturn() " + message);             
            #else
            //Debug.Log("PhotonLiteCommManager: DebugReturn() " + message); 
            #endif
        }

        // Prints a description of the state
        private static string getStateString(State printState)
        {
            switch (printState)
            {
                case State.INITIALIZED:
                    return "State (Initialized)";
                case State.CONNECTING:
                    return "State (Connecting)";
                case State.CONNECTED:
                    return "State (Connected)";
                case State.JOINING:
                    return "State (Joining)";
                case State.JOINED:
                    return "State (Joined)";
                case State.LEAVING:
                    return "State (Leaving)";
                case State.DISCONNECTING:
                    return "State (Disconnecting)";
                case State.DISCONNECTED:
                    return "State (Disconnected)";
                case State.TERMINATED:
                    return "State (Terminated)";
                default:
                    return "State (Unknown)";
            }
        }
    }
}