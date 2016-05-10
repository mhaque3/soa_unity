using UnityEngine;
using System.Collections;
using Gamelogic.Grids;

public class TrackOnGrid : MonoBehaviour 
{
    public SoaHexWorld hexGrid;
    public FlatHexPoint currentCell;
    public SoaActor thisSoaActor;

	// Use this for initialization
	void Start () 
    {
        thisSoaActor = GetComponent<SoaActor>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        hexGrid.UnHighlightCell(currentCell);
        try
        {
            currentCell = hexGrid.Map[transform.position];

            var block = hexGrid.Grid[currentCell];
            if (block.transform.localScale.y > 1)
            {
                if (thisSoaActor != null)
                    thisSoaActor.Kill("Mountain");
            }

            hexGrid.HighlightCell(currentCell);
        }
        catch
        {
            Debug.Log("BAD POINT: " + transform.name + " at " + transform.position);
        }
	}
}
