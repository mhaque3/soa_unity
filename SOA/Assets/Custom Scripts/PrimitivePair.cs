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

        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PrimitivePair<S, T>)
            {
                PrimitivePair<S, T> other = obj as PrimitivePair<S, T>;
                return first.Equals(other.first) && second.Equals(other.second);
            }
            else
            {
                return false;
            }
        }
    }
}
