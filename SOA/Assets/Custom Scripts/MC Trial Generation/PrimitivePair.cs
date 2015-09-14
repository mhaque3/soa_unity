using System;
using System.Linq;
using System.Text;

namespace soa
{
    public class PrimitivePair<S,T>
    {
        // Members
        public S first;
        public T second;

        // Constructor
        public PrimitivePair(S first, T second) 
        {
            this.first = first;
            this.second = second;
        }
    }
}
