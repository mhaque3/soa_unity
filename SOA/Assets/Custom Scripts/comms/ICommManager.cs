using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public interface ICommManager
    {
        void start();

        void terminate();

        void addOutgoing(List<Belief> l, int sourceID, int[] targetActorIDs);

        void addOutgoing(Belief b, int sourceID, int[] targetActorIDs);
    }
}
