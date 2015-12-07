using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace soa
{
    class NetworkTopologyReader
    {
        // Members
        private FileStream fileStream;
        private int N;
        private List<bool[,]> adjHistory;
        private List<bool> bitQueue;

        // Constructor
        public NetworkTopologyReader(string inputFilename)
        {
            // Save arguments
            fileStream = new FileStream(inputFilename, FileMode.Open);

            // Create new list
            adjHistory = new List<bool[,]>();
            bitQueue = new List<bool>();

            // Read and save the data
            Read();
        }

        // Read method
        private void Read()
        {
            // First read the size N
            byte[] intBuffer = new byte[4];
            fileStream.Read(intBuffer, 0, 4);
            N = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(intBuffer,0));

            // Process each adjacency matrix
            int bytesRequested = (int)(Math.Round(1000.0f * (N*N) / 8.0f));
            byte[] byteBuffer = new byte[bytesRequested];
            int bytesRead = bytesRequested;
            bool eof = false;
            while (!eof)
            {
                // Read bytes
                bytesRead = fileStream.Read(byteBuffer, 0, bytesRequested);

                // Convert bytes to bools and add to bitQueue
                BitArray bits = new BitArray(byteBuffer);
                for (int i = 0; i < bytesRead*8; i++)
                {
                    bitQueue.Add(bits[i]);
                }

                // Convert bits to adjacency matrices
                while (bitQueue.Count >= N * N)
                {
                    // Form a temporary adjacency matrix
                    bool[,] tempAdj = new bool[N, N];
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            tempAdj[i, j] = bitQueue[N * i + j];
                        }
                    }
                    bitQueue.RemoveRange(0, N * N);

                    // Add the adj matrix to history
                    adjHistory.Add(tempAdj);
                }

                // Check to see if we've reached end of file
                eof = (bytesRead != bytesRequested);
            }

            // Close the stream
            try
            {
                fileStream.Close();
            }
            catch (Exception)
            {
            }
        }

        // Get number of vectices
        public int GetN()
        {
            return N;
        }

        // Get history of adjacency matrices
        public List<bool[,]> GetAdjacencyHistory()
        {
            // Make a deep copy without altering internal members
            List<bool[,]> returnList = new List<bool[,]>();
            for (int i = 0; i < adjHistory.Count; i++)
            {
                returnList.Add((bool[,])adjHistory[i].Clone());
            }
            return returnList;
        }
    }
}
