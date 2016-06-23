using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace soa
{
    public class ConnectionProtocol
    {
        public const int SERVER_ID = 9999;
        private const int HEADER_LENGTH = 8;
        private const int HEADER_TYPE_OFFSET = 0;
        private const int HEADER_SOURCE_OFFSET = 4;

        public class RequestData
        {
            public IPEndPoint address;
            public RequestType type;
            public int sourceID;
            public byte[] messageData;

            public RequestData()
            {
                this.sourceID = -1;
                this.type = RequestType.UNKNOWN;
                this.messageData = null;
            }
        }

        public enum RequestType
        {
            CONNECT=0,
            POST=1,
            UNKNOWN=-1
        }
        
        public Message formatMessage(RequestData data)
        {
            byte[] messageData = new byte[HEADER_LENGTH + data.messageData.Length];
            writeInt32(messageData, HEADER_TYPE_OFFSET, (int)data.type);
            writeInt32(messageData, HEADER_SOURCE_OFFSET, data.sourceID);
            
            System.Buffer.BlockCopy(data.messageData, 0, messageData, HEADER_LENGTH, data.messageData.Length);

            return new Message(data.address, messageData);
        }

        public RequestData parse(Message message)
        {
            RequestData data = new RequestData();
            data.address = message.address;

            if (message.data.Length < HEADER_LENGTH)
            {
                throw new Exception("Invalid message: " + System.Text.Encoding.Default.GetString(message.data));
            }

            int messageType = parseInt32(message.data, HEADER_TYPE_OFFSET);
            if (Enum.IsDefined(typeof(RequestType), messageType))
            {
                data.type = (RequestType)messageType;
            }

            data.sourceID = parseInt32(message.data, HEADER_SOURCE_OFFSET);

            int bytesRemaining = message.data.Length - HEADER_LENGTH;
            data.messageData = new byte[bytesRemaining];
            if (bytesRemaining > 0)
            {   
                System.Buffer.BlockCopy(message.data, HEADER_LENGTH, data.messageData, 0, bytesRemaining);
            }

            return data;
        }

        private int parseInt32(byte[] buffer, int startIndex)
        {
            int value = BitConverter.ToInt32(buffer, startIndex);
            return IPAddress.NetworkToHostOrder(value);
        }

        private void writeInt32(byte[] buffer, int startIndex, int value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            byte[] bytes = BitConverter.GetBytes(value);
            
            for (int i = 0;i < bytes.Length; ++i)
            {
                buffer[i + startIndex] = bytes[i];
            }
        }
    }
}
