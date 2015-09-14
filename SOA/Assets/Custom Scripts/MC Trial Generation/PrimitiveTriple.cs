using System;
using System.Linq;
using System.Text;

namespace soa
{
    public class PrimitiveTriple<S,T,U>
    {
        // Members
        public S first;
        public T second;
        public U third;

        // Constructor
        public PrimitiveTriple(S first, T second, U third) 
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }
}
