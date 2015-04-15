using System.Linq;
using Gamelogic;
using Gamelogic.Grids;
using UnityEngine;

public class PathFindingHexGrid : GridBehaviour<PointyHexPoint>
{
	public SpriteCell pathPrefab;
	public GameObject pathRoot;

	private PointyHexPoint start;
	private PointyHexPoint goal;
	private bool selectStart = true; //otherwise, select goal

	private PointyHexGrid<WalkableCell> walkableGrid; 

	override public void InitGrid()
	{
		//We cast the grid and grid values here for convenience.
		//Casting like this clones the grid. Thus neighbor relations are 
		//not preserved (this is a design flaw as of Grids 1.8, to be fixed
		//in a future version). For now it means the actual pathfinding call
		//must use the original grid.
		walkableGrid = (PointyHexGrid<WalkableCell>) Grid.CastValues<WalkableCell, PointyHexPoint>();

		foreach (var point in walkableGrid)
		{
			walkableGrid[point].IsWalkable = true;
		}

		start = walkableGrid.First();
		goal = walkableGrid.Last();

		UpdatePath();
	}

	//This is the distance function used by the path algorithm.
	private float EuclideanDistance(PointyHexPoint p, PointyHexPoint q)
	{
		float distance = (Map[p] - Map[q]).magnitude;

		return distance;
	}

	public void OnLeftClick(PointyHexPoint selectedPoint)
	{
		ToggleCellWalkability(selectedPoint);
		UpdatePath();
	}

	public void OnRightClick(PointyHexPoint selectedPoint)
	{
		SetStartOrGoal(selectedPoint);
		UpdatePath();
	}

	private void ToggleCellWalkability(PointyHexPoint selectedPoint)
	{
		walkableGrid[selectedPoint].IsWalkable = !walkableGrid[selectedPoint].IsWalkable;

		var color = walkableGrid[selectedPoint].IsWalkable ? ExampleUtils.Colors[0] : Color.black;
		walkableGrid[selectedPoint].Color = color;
	}

	private void SetStartOrGoal(PointyHexPoint selectedPoint)
	{
		if (selectStart && selectedPoint != goal)
		{
			start = selectedPoint;
			selectStart = false;
		}
		else if (selectedPoint != start)
		{
			goal = selectedPoint;
			selectStart = true;
		}
	}

	private void UpdatePath()
	{
		if (Application.isPlaying)
		{
			pathRoot.transform.DestroyChildren();
		}
		else
		{
			pathRoot.transform.DestroyChildrenImmediate();
		}

		//We use the original grid here, and not the 
		//copy, to preserve neighbor relationships. Therefore, we
		//have to cast the cell in the lambda expression below.
		var path = Algorithms.AStar(
				Grid,
				start,
				goal,
				EuclideanDistance,
				c => ((WalkableCell) c).IsWalkable,
				EuclideanDistance);

		if (path == null)
		{
			return; //then there is no path between the start and goal.
		}

		foreach (var point in path)
		{
			var pathNode = Instantiate(pathPrefab);

			pathNode.transform.parent = pathRoot.transform;
			pathNode.transform.localScale = Vector3.one * 0.5f;
			pathNode.transform.localPosition = Map[point];

			if (point == start)
			{
				pathNode.Color = ExampleUtils.Colors[1];
			}
			else if (point == goal)
			{
				pathNode.Color = ExampleUtils.Colors[3];
			}
			else
			{
				pathNode.Color = ExampleUtils.Colors[2];
			}
		}
	}
}
