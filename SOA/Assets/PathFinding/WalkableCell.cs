using Gamelogic.Grids;

public class WalkableCell : SpriteCell
{
	private bool isWalkable = true;

	public bool IsWalkable
	{
		get { return isWalkable; }
		set { isWalkable = value; }
	}

	//Defined to disable default highlight behaviour
	new public void OnClick()
	{ }
}
