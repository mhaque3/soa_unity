using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gamelogic.Grids;

public class HexFieldMesh : MonoBehaviour
{
    #region Constants
    private static readonly float Sqrt3 = Mathf.Sqrt(3);

    private static readonly Vector3[] VertexDirections =
   {
      new Vector3(0, 0, 1f/2),
      new Vector3(Sqrt3/4, 0, 1f/4),
      new Vector3(Sqrt3/4, 0, -1f/4),
      new Vector3(0, 0, -1f/2),
      new Vector3(-Sqrt3/4, 0, -1f/4),
      new Vector3(-Sqrt3/4, 0, 1f/4),
      Vector3.zero
   };

    private static readonly Vector2[] UVDirections =
       VertexDirections.Select(v => new Vector2(v.x, v.z) + Vector2.one * 0.5f).ToArray();

    private static readonly int[] Triangles =
   {
      6, 0, 1,
      6, 1, 2,
      6, 2, 3,
      6, 3, 4,
      6, 4, 5,
      6, 5, 0
   };

    private static readonly Vector3[] Normals = new Vector3[]
   {
      Vector3.up,
      Vector3.up,
      Vector3.up,
      Vector3.up,
      Vector3.up,
      Vector3.up,
      Vector3.up
   };

    //Tweakables
    //Assmes the texture you have on you renderer is divided 
    //in 5x4 rectangles. A texture like this: http://www.andrethiel.de/ContentImages/2D/Fullsize/Textures_Terrain.jpg
    private const int textureGridWidth = 4;
    private const int textureGridHeight = 3;

    const float textureCellWidth = 1f / textureGridWidth;
    const float textureCellHeight = 1f / textureGridHeight;

    const int textureCount = textureGridWidth * textureGridHeight;
    #endregion

    #region Menu commands
    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        var grid = PointyHexGrid<int>.Hexagon(50);

        foreach (var point in grid)
        {
            grid[point] = Random.Range(0, textureCount);
        }

        var dimensions = new Vector2(69, 80);

        var map = new PointyHexMap(dimensions)
           .WithWindow(new Rect(0, 0, 0, 0))
           .AlignMiddleCenter(grid)
           .To3DXZ();

        var mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;

        GenerateMesh(mesh, grid, map, dimensions);
    }
    #endregion

    #region Implementation
    private static void GenerateMesh(Mesh mesh, IGrid<int, PointyHexPoint> grid, IMap3D<PointyHexPoint> map, Vector2 dimensions)
    {
        mesh.Clear();
        mesh.vertices = MakeVertices(grid, map, dimensions);
        mesh.uv = MakeUVs(grid);
        mesh.triangles = MakeTriangles(grid);
        mesh.normals = MakeNormals(grid);
    }

    private static Vector3[] MakeNormals(IEnumerable<PointyHexPoint> grid)
    {
        return grid.SelectMany(p => Normals).ToArray();
    }

    private static int[] MakeTriangles(IEnumerable<PointyHexPoint> grid)
    {
        var vertexIndices = Enumerable.Range(0, grid.Count());

        return vertexIndices
           .SelectMany(i => Triangles.Select(j => i * 7 + j))
           .ToArray();
    }

    private static Vector2[] MakeUVs(IGrid<int, PointyHexPoint> grid)
    {
        return grid
           .SelectMany(p => UVDirections.Select(uv => CalcUV(uv, grid[p])))
           .ToArray();
    }

    private static Vector3[] MakeVertices(IEnumerable<PointyHexPoint> grid, IMap3D<PointyHexPoint> map, Vector2 dimensions)
    {
        return grid
           .SelectMany(p => VertexDirections.Select(v => v * dimensions.y + map[p]))
           .ToArray();
    }

    private static Vector2 CalcUV(Vector2 fullUV, int textureIndex)
    {
        int textureIndexX = textureIndex % textureGridWidth;
        int textureIndexY = textureIndex / textureGridHeight;

        float u = fullUV.x / textureGridWidth + textureIndexX * textureCellWidth;
        float v = fullUV.y / textureGridHeight + textureIndexY * textureCellHeight;

        return new Vector2(u, v);
    }
    #endregion
}

