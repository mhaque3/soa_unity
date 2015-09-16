using System.Linq;
using Gamelogic.Grids;
//----------------------------------------------//
// Gamelogic Grids                              //
// http://www.gamelogic.co.za                   //
// Copyright (c) 2013 Gamelogic (Pty) Ltd       //
//----------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

/**
	This example shows how you can use a grid in 3D.
*/
public class SoaHexWorld : GridBehaviour<FlatHexPoint>
{
	public Texture2D heightMap;
    public Texture2D politicalMap;
    public float HeightScale;
    public Material baseMaterial;
    public Material highlightMaterial;
    public Material baseGrass;
    public Material redGrass;
    public Material baseRockyGrass;
    public Material redRockyGrass;
    public Material baseRocky;
    public Material baseSandy;
    public Material baseWater;

    public float WaterHeight = 0.5f;
    public float BaseHeight = 1.0f;
    public float MountainHeight = 2.0f;

    public List<FlatHexPoint> WaterHexes;
    public List<FlatHexPoint> MountainHexes;
    public List<FlatHexPoint> LandHexes;

	override public void InitGrid()
	{
		var imageRectHeight = new Rect(0, 0, heightMap.width, heightMap.height);
        var imageRectPolitical = new Rect(0, 0, politicalMap.width, politicalMap.height);
		var map = new FlatHexMap(new Vector2(80, 69));
        var mapHeight = new ImageMap<FlatHexPoint>(imageRectHeight, Grid, map);
        var mapPolitical = new ImageMap<FlatHexPoint>(imageRectPolitical, Grid, map);

        WaterHexes = new List<FlatHexPoint>();
        MountainHexes = new List<FlatHexPoint>();
        LandHexes = new List<FlatHexPoint>();

		foreach (var point in Grid)
		{
            int x = Mathf.FloorToInt(mapHeight[point].x);
            int y = Mathf.FloorToInt(mapHeight[point].y);
            float height = heightMap.GetPixel(x, y).r * HeightScale;

			if (height <= 0)
			{
				height = 0.01f;
			}


			var block = Grid[point];

			if (block == null) Debug.LogWarning("block is null " + point);
			else
			{

                if (height < HeightScale * 0.3f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseWater;
                    block.GetComponent<Renderer>().material = baseWater;
                    block.transform.localScale = new Vector3(1, WaterHeight, 1);
                    //block.transform.position += new Vector3(0f, (BaseHeight + WaterHeight) / 4f, 0f);
                    WaterHexes.Add(point);
                }
                else if (height > HeightScale * 0.6f && height < HeightScale * 0.7f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseGrass;
                    block.transform.localScale = new Vector3(1, BaseHeight, 1);
                    LandHexes.Add(point);
                }
                else if (height >= HeightScale * 0.7f)
                {
                    UVCell thisHex = block.GetComponentInChildren<UVCell>();
                    thisHex.GetComponent<Renderer>().material = baseRocky;
                    block.transform.localScale = new Vector3(1, MountainHeight, 1);
                    MountainHexes.Add(point);
                }
                else
                {
                    // Also land?
                    // Why are these different than the 0.6 to 0.7 heights?
                    // Question for Bob
                    LandHexes.Add(point);
                }

			}
		}
    }

    public float KmToUnity()
    {
        FlatHexPoint p0 = new FlatHexPoint(0, 0);
        FlatHexPoint p1 = new FlatHexPoint(0, 1);

        return (Map[p1] - Map[p0]).magnitude;
    }

    public void HighlightCell(FlatHexPoint point)
    {
        Block thisBlock = (Block)Grid[point];
        if (thisBlock != null)
        {
            thisBlock.GetComponent<Renderer>().material = highlightMaterial;
            //UVCell thisHex = thisBlock.GetComponentInChildren<UVCell>();
            //thisHex.GetComponent<Renderer>().material = redGrass;
        }
    }

    public void UnHighlightCell(FlatHexPoint point)
    {
        Block thisBlock = (Block)Grid[point];
        if (thisBlock != null)
        {
            if (thisBlock.transform.localScale.y == WaterHeight)
                thisBlock.GetComponent<Renderer>().material = baseWater;
            if (thisBlock.transform.localScale.y == BaseHeight)
                thisBlock.GetComponent<Renderer>().material = baseMaterial;
            if (thisBlock.transform.localScale.y == MountainHeight)
                thisBlock.GetComponent<Renderer>().material = baseMaterial;
            //UVCell thisHex = thisBlock.GetComponentInChildren<UVCell>();
            //thisHex.GetComponent<Renderer>().material = baseGrass;
        }
    }

}
