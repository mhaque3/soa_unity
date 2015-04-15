using UnityEngine;
using System.Collections;
using Gamelogic.Grids;

public class TrackOnGrid : MonoBehaviour 
{
    public SoaHexWorld hexGrid;
    public FlatHexPoint currentCell;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        hexGrid.UnHighlightCell(currentCell);
        try
        {
            currentCell = hexGrid.Map[transform.position];
            hexGrid.HighlightCell(currentCell);
        }
        catch
        {
            Debug.Log("BAD POINT: " + transform.name + " at " + transform.position);
        }
	}
}
