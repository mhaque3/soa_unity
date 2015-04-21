// Additinonal using statements are needed if we are running in Unity
#if(NOT_UNITY)
#else
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;

namespace soa
{
    /// <summary>
    /// Facilitates communication of Belief objects over Photon Cloud network.
    /// </summary>
    public class PhotonCloudCommManager : LoadBalancingClient
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

        // Photon
        private Thread photonUpdateThread;

        // Keep track of internal states
        private ClientState prevState;

        // Settings for managing outgoing queue
        private int maxQueueSize;
        private bool overwriteWhenQueueFull;

        // Internal State
        private bool terminateRequested, startEligible;
        private System.Object startEligibleMutex;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotonCloudCommManager"/> class. 
        /// </summary>
        public PhotonCloudCommManager(DataManager dataManager, Serializer serializer,
            string ipAddress, string roomName, int defaultSourceID, int updateSleepTime_ms = 100,
            int maxQueueSize = 1000000, bool overwriteWhenQueueFull = true)
            : base()
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

            // Initialize prev states
            prevState = State;

            // Used to keep track of whether a new thread can start
            startEligible = true;

            // Used to keep track of whether termination request has been sent
            terminateRequested = false;

            // Initialize thread references
            photonUpdateThread = null;

            // Insert your game's AppID (replace <InsertYourAppIdHere>). 
            // Hosting yourself: use any name. 
            // Using Photon Cloud: use your cloud subscription's appID
            this.AppId = "36a65149-f55c-4395-b2db-33bff4de9282";
            this.AppVersion = "1.0";
            this.MasterServerAddress = ipAddress;
        }

        /// <summary>
        /// Starts the communication manager 
        /// </summary>
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

                    // Create and start new thread
                    photonUpdateThread = new Thread(new ThreadStart(photonUpdate));
                    photonUpdateThread.Start();

                    // Wait for it to start up
                    while (!photonUpdateThread.IsAlive) ;
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonCloudCommManager: Update thread started");
                    #else
                    Debug.Log("PhotonCloudCommManager: Update thread started");
                    #endif
                }
                else
                {
                    // Photon update thread already running, no need to do anything else
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonCloudCommManager: start() has no effect, update thread already running");
                    #else
                    Debug.Log("PhotonCloudCommManager: start() has no effect, update thread already running");
                    #endif
                }
            } // Unlock
        }

        /// <summary>
        /// Sends signal to shut down the communication manager 
        /// </summary>
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
                    Console.WriteLine("PhotonCloudCommManager: Termination request sent");
                    #else
                    Debug.Log("PhotonCloudCommManager: Termination request sent");
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
                    Console.WriteLine("PhotonCloudCommManager: terminate() has no effect, update thread already inactive");
                    #else
                    Debug.Log("PhotonCloudCommManager: terminate() has no effect, update thread already inactive");
                    #endif
                }
            } // Unlock
        }

        /// <summary>
        /// Thread method that implements a finite InternalState machine to connect to photon server and send outgoing messages periodically
        /// </summary>
        private void photonUpdate()
        {
            // Keep looping until conditions are met to terminate and exit fsm
            while (!(terminateRequested && (State == ClientState.Uninitialized || State == ClientState.Disconnected)))
            {
                // Print internal state
                if (prevState != State)
                {
                    #if(NOT_UNITY)
                    Console.WriteLine("PhotonCloudCommManager: " + prevState + " -> " + State);                  
                    #else
                    Debug.Log("PhotonCloudCommManager: " + prevState + " -> " + State);
                    #endif
                    prevState = State;
                }

                if (terminateRequested)
                {
                    // Just a single call to disconnect
                    Disconnect();
                }
                else
                {
                    // Normal operation, take appropriate action based on current connectivity state
                    switch (State)
                    {
                        case ClientState.Disconnected:
                        case ClientState.Uninitialized:
                            // Connect to a Photon server
                            Connect();
                            break;
                        case ClientState.JoinedLobby:
                            // Create or join our room
                            OpJoinOrCreateRoom(roomName, 0, new RoomOptions());
                            break;
                        case ClientState.Joined:
                            // Within room, send any queued outgoing messages
                            sendOutgoing();
                            break;
                    }
                }

                // Communicate with Photon server
                Service();

                // Wait a while before talking with server again
                System.Threading.Thread.Sleep(updateSleepTime_ms);
            }
        }

        /// <summary>
        /// Adds information from data manager to outgoing queue using default source ID
        /// </summary>
        public void addOutgoing(Belief b)
        {
            addOutgoing(b, defaultSourceID);
        }

        /// <summary>
        /// Adds information from data manager to outgoing queue using specified source ID
        /// </summary>
        public void addOutgoing(Belief b, int sourceID)
        {
            // Serialize the source ID into 4 bytes, network byte order (Big Endian)
            Byte[] sourceIDBytes = BitConverter.GetBytes(sourceID);
            
            // Serialize the belief
            Byte[] beliefBytes = serializer.serializeBelief(b);

            // Enqueue the serialized message if serialization was
            // successful (if serial is nonempty)
            if (beliefBytes.Length > 0)
            {
                // Combine the serialized source ID and belief into one message
                Byte[] message = new Byte[sourceIDBytes.Length + beliefBytes.Length];
                System.Buffer.BlockCopy(sourceIDBytes, 0, message, 0, sourceIDBytes.Length);
                System.Buffer.BlockCopy(beliefBytes, 0, message, sourceIDBytes.Length, beliefBytes.Length);
                
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
                Console.Error.WriteLine("PhotonCloudCommManager: Belief serialization failed");
                #else
                Debug.Log("PhotonCloudCommManager: Belief serialization failed");
                #endif
            }
        }

        /// <summary>
        /// Sends any outgoing messages that are on the queue
        /// </summary>
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
                //op/Params[ParameterCode.Code] = (byte)0;
                Hashtable evData = new Hashtable();
                evData[(byte)0] = message;
                //opParams[ParameterCode.Data] = evData;
                //peer.OpCustom((byte)LiteOpCode.RaiseEvent, opParams, true);
                OpRaiseEvent(0, evData, true, null);
            }
        }

        /// <summary>
        /// Callback to handle messages received over Photon game network
        /// Processed within PhotonPeer.DispatchIncomingCommands()!
        /// </summary>
        public override void OnEvent(EventData photonEvent)
        {
            // Call original OnEvent()
            base.OnEvent(photonEvent);

            // Handle our custom event
            switch (photonEvent.Code)
            {
                case 0: // Received a Protobuff message
                    {
                        // Get the byte[] message from protobuf
                        // Message = 1 byte for length of rest of message, 4 bytes for source ID, rest is serialized belief
                        Hashtable evData = (Hashtable)photonEvent.Parameters[ParameterCode.Data];
                        Byte[] message = (Byte[])evData[(byte)0];

                        // Extract belief length and source ID
                        int serializedBeliefLength = (int)message[0] - 4;
                        int sourceID = BitConverter.ToInt32(message, 1); // get from bytes 1 through 4 in array

                        // Extract serialized belief
                        Byte[] serializedBelief = new Byte[serializedBeliefLength];
                        System.Buffer.BlockCopy(message, 5, serializedBelief, 0, serializedBeliefLength);

                        // Generate the belief
                        Belief b = serializer.generateBelief(serializedBelief);

                        // If deserialization was successful
                        if (b != null)
                        {
                            // Filter the belief
                            #if(NOT_UNITY)
                            if (dataManager.filterBelief(b))
                            {
                            #endif
                                // Add the belief to the data manager if it passed filter, no filter for Unity
                                dataManager.addBelief(b, sourceID);
                            #if(NOT_UNITY)
                            }
                            #endif
                        }
                        else
                        {
                            // Something went wrong with deserialization
                            #if(NOT_UNITY)
                            Console.Error.WriteLine("PhotonCloudCommManager: Belief deserialization failed");
                            #else
                            Debug.Log("PhotonCloudCommManager: Belief deserialization failed");
                            #endif
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Write to console for debugging output. 
        /// </summary>
        private void DebugReturn(string debugStr)
        {
            #if(NOT_UNITY)
            //Console.WriteLine(debugStr);            
            #else
            //Debug.Log(debugStr);
            #endif
        }
    }
}
