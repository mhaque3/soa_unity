using System.Linq;
using Gamelogic.Grids;
//----------------------------------------------//
// Gamelogic Grids                              //
// http://www.gamelogic.co.za                   //
// Copyright (c) 2013 Gamelogic (Pty) Ltd       //
//----------------------------------------------//
using UnityEngine;

/**
	This example shows how you can use a grid in 3D.
*/
public class SoaHexWorldFlat: GridBehaviour<FlatHexPoint>
{
	public Texture2D heightMap;
    public float HeightScale;
    public Material baseMaterial;
    public Material highlightMaterial;
    public Material baseGrass;
    public Material redGrass;
    public Material baseRockyGrass;
    public Material redRockyGrass;
    public Material baseRocky;
    public Material baseSandy;

	override public void InitGrid()
	{
		var imageRect = new Rect(0, 0, heightMap.width, heightMap.height);
		var map = new FlatHexMap(new Vector2(80, 69));
		var map2D = new ImageMap<FlatHexPoint>(imageRect, Grid, map);

		foreach (var point in Grid)
		{
			int x = Mathf.FloorToInt(map2D[point].x);
			int y = Mathf.FloorToInt(map2D[point].y);
            float height = heightMap.GetPixel(x, y).r * HeightScale;

			if (height <= 0)
			{
				height = 0.01f;
			}


			var block = Grid[point];

			if (block == null) Debug.LogWarning("block is null " + point);
			else
			{
				//block.Color = ExampleUtils.Blend(height, ExampleUtils.DefaultColors[0], ExampleUtils.DefaultColors[1]);
				//block.transform.localScale = new Vector3(1, height, 1);


                if (height < HeightScale * 0.3f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseSandy;
                }
                if (height > HeightScale * 0.6f && height < HeightScale * 0.8f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseRockyGrass;
                }
                if (height >= HeightScale * 0.8f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseRocky;
                }
			}
		}
	}

    public void HighlightCell(FlatHexPoint point)
    {
        //Block thisBlock = (Block)Grid[point];  
        //thisBlock.GetComponent<Renderer>().material = highlightMaterial;
        //UVCell thisHex = thisBlock.GetComponentInChildren<UVCell>();
        //thisHex.GetComponent<Renderer>().material = redGrass;
    }

    public void UnHighlightCell(FlatHexPoint point)
    {
        //Block thisBlock = (Block)Grid[point];
        //thisBlock.GetComponent<Renderer>().material = baseMaterial;
        //UVCell thisHex = thisBlock.GetComponentInChildren<UVCell>();
        //thisHex.GetComponent<Renderer>().material = baseGrass;
    }

}
