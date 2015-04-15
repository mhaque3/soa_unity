using System.Collections.Generic;
using Gamelogic.Grids;


namespace Gamelogic.Grids
{
	public interface IShape<TPoint> : IEnumerable<TPoint>
		where TPoint : IGridPoint<TPoint>
	{
		bool Contains(TPoint point);
	}
}
