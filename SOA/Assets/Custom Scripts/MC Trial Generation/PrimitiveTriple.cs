using System;
using System.Linq;
using System.Text;

namespace soa
{
    public class PrimitiveTriple<S, T, U>
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

        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode() ^ third.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PrimitiveTriple<S, T, U>)
            {
                PrimitiveTriple<S, T, U> other = obj as PrimitiveTriple<S, T, U>;
                return first.Equals(other.first) && second.Equals(other.second) && third.Equals(other.third);
            }
            else
            {
                return false;
            }
        }
    }
}
