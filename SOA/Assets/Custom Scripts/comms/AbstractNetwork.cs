using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public abstract class AbstractNetwork
    {
        public abstract void Send(byte[] buffer, int length);
        
        public abstract byte[] Receive();
    }
}
