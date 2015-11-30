using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace soa
{
    class NetworkTopologyWriter
    {
        // Members
        private bool newStream;
        private int N;
        private List<bool> bitQueue;
        private bool[] bitBuffer;
        private FileStream fileStream;
        private BufferedStream bufferedStream;
        private byte[] tempByte;

        // Constructor
        public NetworkTopologyWriter(string outputFilename)
        {
            // Save arguments
            fileStream = new FileStream(outputFilename, FileMode.Create);
            bufferedStream = new BufferedStream(fileStream);
      
            // Specify that it is a new stream
            newStream = true;

            // Initialize lists
            bitQueue = new List<bool>();

            // Initialize temp storage
            bitBuffer = new bool[8];
            tempByte = new byte[1];
        }

        public void Add(bool[,] adjMatrix)
        {
            // Save and also write out the size of adj matrix if new stream
            lock (bitQueue)
            {
                if (newStream)
                {
                    // Save matrix size
                    N = adjMatrix.GetLength(0);

                    // Write to stream in network order
                    bufferedStream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(N)), 0, 4);

                    // No longer a new stream
                    newStream = false;
                }

                // Convert adj matrix to byte array and add to queue
                EnqueueBitArray(FlattenAdjMatrix(adjMatrix));

                // Try to write contents to stream
                Write();
            }
        }

        // Tries to write what is in the bit array as bytes into stream
        private void Write()
        {
            lock (bitQueue)
            {
                while (bitQueue.Count > 8)
                {
                    // Copy 8 bits over to buffer and remove from the queue
                    bitQueue.CopyTo(0, bitBuffer, 0, 8);
                    bitQueue.RemoveRange(0, 8);

                    // Write a byte from bitBuffer
                    WriteByte();
                }
            }
        }

        private void WriteByte()
        {
            // Write a single byte from bitBuffer to the bufferedStream
            new BitArray(bitBuffer).CopyTo(tempByte, 0);
            bufferedStream.WriteByte(tempByte[0]);
        }

        // Writes what is left, flushes, and closes
        public void Close()
        {
            lock (bitQueue)
            {
                if (bitQueue.Count > 0)
                {
                    // Write what is left and pad with false
                    for (int i = 0; i < bitQueue.Count; i++)
                    {
                        bitBuffer[i] = bitQueue[i];
                    }
                    for (int i = bitQueue.Count; i < 8; i++)
                    {
                        bitBuffer[i] = false;
                    }

                    // Write the last byte
                    WriteByte();

                    // Clear the bit queue
                    bitQueue.Clear();
                }

                // Flush and close streams
                try
                {
                    bufferedStream.Flush();
                    bufferedStream.Close();
                }
                catch (Exception)
                {
                }
                try
                {
                    fileStream.Flush();
                    fileStream.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        // Extracts contents of adj matrix and puts in bit array
        private bool[] FlattenAdjMatrix(bool[,] adjMatrix)
        {
            // Flatten 2D array
            bool[] flattened = new bool[N * N];
            Buffer.BlockCopy(adjMatrix, 0, flattened, 0, N * N);
 
            // Convert to bit array
            return flattened;
        }

        // Thread safe enqueue to bit array
        private void EnqueueBitArray(bool[] b)
        {
            lock(bitQueue)
            {
                bitQueue.AddRange(b);
            }
        }
    }
}
