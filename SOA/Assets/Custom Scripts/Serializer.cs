using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public abstract class Serializer
    {
        // Belief to serialized string
        public abstract Byte[] serializeBelief(Belief b);

        // Serialized string to Belief conversion
        public abstract Belief generateBelief(Byte[] serial);
    }
}
