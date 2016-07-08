using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    
    public interface IPhysicalLayer<AddressType>
    {

        Communicator<AddressType> BuildCommunicatorFor(int actorID);

        void Update();

        void AddActor(SoaActor actor);

        void RemoveActor(SoaActor actor);
    }
}
