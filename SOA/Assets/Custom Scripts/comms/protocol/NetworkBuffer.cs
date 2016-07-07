using System;

namespace soa
{
	public class NetworkBuffer
	{

		private byte[] data;

		public NetworkBuffer(byte[] data)
		{
			this.data = data;
		}

		public NetworkBuffer(int bufferLength)
		{
			data = new byte[bufferLength];
		}

		public uint size() 
		{
			return data.Length;
		}

		public byte[] getBuffer()
		{
			return data;
		}

		public void writeBytes(byte[] source, int sourceIndex, int destIndex, int length)
		{
			System.Buffer.BlockCopy(source, sourceIndex, data, destIndex, length);
		}

		public int parseInt32(int startIndex)
        {
            int value = BitConverter.ToInt32(data, startIndex);
            return IPAddress.NetworkToHostOrder(value);
        }

        public void writeInt32(int startIndex, int value)
        {
            value = IPAddress.HostToNetworkOrder(value);
            byte[] bytes = BitConverter.GetBytes(value);

            for (int i = 0;i < bytes.Length; ++i)
            {
				data[startIndex + i] = bytes[i];
            }
        }
	}
}

