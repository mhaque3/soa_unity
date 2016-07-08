using System;
using System.Collections.Generic;

namespace soa
{
	public interface IWorld
	{
		IEnumerable<ISoaActor> getRedActors();
		
		IEnumerable<ISoaActor> getBlueActors();
		
		IEnumerable<ISoaJammer> getJammers();	
	}
}