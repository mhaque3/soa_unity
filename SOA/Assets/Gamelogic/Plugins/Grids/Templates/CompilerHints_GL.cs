//----------------------------------------------//
// Gamelogic Grids                              //
// http://www.gamelogic.co.za                   //
// Copyright (c) 2013 Gamelogic (Pty) Ltd       //
//----------------------------------------------//

// Auto-generated File

using System.Linq;

namespace Gamelogic.Grids
{
	/**
		Compiler hints for our examples.

		@since 1.8
	*/
	public static class __CompilerHintsGL
	{
		public static bool __CompilerHint__Rect__TileCell()
		{
			var grid = new RectGrid<TileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new TileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<RectPoint>(new IntRect(), p => true);
			var shapeInfo = new RectShapeInfo<TileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(RectPoint.Zero) != null;
		}
		public static bool __CompilerHint__Diamond__TileCell()
		{
			var grid = new DiamondGrid<TileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new TileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<DiamondPoint>(new IntRect(), p => true);
			var shapeInfo = new DiamondShapeInfo<TileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(DiamondPoint.Zero) != null;
		}
		public static bool __CompilerHint__PointyHex__TileCell()
		{
			var grid = new PointyHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new TileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyHexPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyHexShapeInfo<TileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(PointyHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatHex__TileCell()
		{
			var grid = new FlatHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new TileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatHexPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatHexShapeInfo<TileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(FlatHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__Line__TileCell()
		{
			var grid = new LineGrid<TileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new TileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<LinePoint>(new IntRect(), p => true);
			var shapeInfo = new LineShapeInfo<TileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(LinePoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatTri__TileCell()
		{
			var grid1 = new PointyHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new TileCell[1]; } 

			var grid2 = new FlatTriGrid<TileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatTriPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatTriShapeInfo<TileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyTri__TileCell()
		{
			var grid1 = new FlatHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new TileCell[1]; } 

			var grid2 = new PointyTriGrid<TileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyTriPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyTriShapeInfo<TileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__FlatRhomb__TileCell()
		{
			var grid1 = new FlatHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new TileCell[1]; } 

			var grid2 = new FlatRhombGrid<TileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatRhombShapeInfo<TileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyRhomb__TileCell()
		{
			var grid1 = new PointyHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new TileCell[1]; } 

			var grid2 = new PointyRhombGrid<TileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyRhombShapeInfo<TileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Cairo__TileCell()
		{
			var grid1 = new PointyHexGrid<TileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new TileCell[1]; } 

			var grid2 = new CairoGrid<TileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<CairoPoint>(new IntRect(), p => true);
			var shapeInfo = new CairoShapeInfo<TileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Rect__SpriteCell()
		{
			var grid = new RectGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new SpriteCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<RectPoint>(new IntRect(), p => true);
			var shapeInfo = new RectShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(RectPoint.Zero) != null;
		}
		public static bool __CompilerHint__Diamond__SpriteCell()
		{
			var grid = new DiamondGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new SpriteCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<DiamondPoint>(new IntRect(), p => true);
			var shapeInfo = new DiamondShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(DiamondPoint.Zero) != null;
		}
		public static bool __CompilerHint__PointyHex__SpriteCell()
		{
			var grid = new PointyHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new SpriteCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyHexPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyHexShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(PointyHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatHex__SpriteCell()
		{
			var grid = new FlatHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new SpriteCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatHexPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatHexShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(FlatHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__Line__SpriteCell()
		{
			var grid = new LineGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new SpriteCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<LinePoint>(new IntRect(), p => true);
			var shapeInfo = new LineShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(LinePoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatTri__SpriteCell()
		{
			var grid1 = new PointyHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new SpriteCell[1]; } 

			var grid2 = new FlatTriGrid<SpriteCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatTriPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatTriShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyTri__SpriteCell()
		{
			var grid1 = new FlatHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new SpriteCell[1]; } 

			var grid2 = new PointyTriGrid<SpriteCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyTriPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyTriShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__FlatRhomb__SpriteCell()
		{
			var grid1 = new FlatHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new SpriteCell[1]; } 

			var grid2 = new FlatRhombGrid<SpriteCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatRhombShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyRhomb__SpriteCell()
		{
			var grid1 = new PointyHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new SpriteCell[1]; } 

			var grid2 = new PointyRhombGrid<SpriteCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyRhombShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Cairo__SpriteCell()
		{
			var grid1 = new PointyHexGrid<SpriteCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new SpriteCell[1]; } 

			var grid2 = new CairoGrid<SpriteCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<CairoPoint>(new IntRect(), p => true);
			var shapeInfo = new CairoShapeInfo<SpriteCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Rect__UVCell()
		{
			var grid = new RectGrid<UVCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new UVCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<RectPoint>(new IntRect(), p => true);
			var shapeInfo = new RectShapeInfo<UVCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(RectPoint.Zero) != null;
		}
		public static bool __CompilerHint__Diamond__UVCell()
		{
			var grid = new DiamondGrid<UVCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new UVCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<DiamondPoint>(new IntRect(), p => true);
			var shapeInfo = new DiamondShapeInfo<UVCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(DiamondPoint.Zero) != null;
		}
		public static bool __CompilerHint__PointyHex__UVCell()
		{
			var grid = new PointyHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new UVCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyHexPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyHexShapeInfo<UVCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(PointyHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatHex__UVCell()
		{
			var grid = new FlatHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new UVCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatHexPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatHexShapeInfo<UVCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(FlatHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__Line__UVCell()
		{
			var grid = new LineGrid<UVCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new UVCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<LinePoint>(new IntRect(), p => true);
			var shapeInfo = new LineShapeInfo<UVCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(LinePoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatTri__UVCell()
		{
			var grid1 = new PointyHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new UVCell[1]; } 

			var grid2 = new FlatTriGrid<UVCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatTriPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatTriShapeInfo<UVCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyTri__UVCell()
		{
			var grid1 = new FlatHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new UVCell[1]; } 

			var grid2 = new PointyTriGrid<UVCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyTriPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyTriShapeInfo<UVCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__FlatRhomb__UVCell()
		{
			var grid1 = new FlatHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new UVCell[1]; } 

			var grid2 = new FlatRhombGrid<UVCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatRhombShapeInfo<UVCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyRhomb__UVCell()
		{
			var grid1 = new PointyHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new UVCell[1]; } 

			var grid2 = new PointyRhombGrid<UVCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyRhombShapeInfo<UVCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Cairo__UVCell()
		{
			var grid1 = new PointyHexGrid<UVCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new UVCell[1]; } 

			var grid2 = new CairoGrid<UVCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<CairoPoint>(new IntRect(), p => true);
			var shapeInfo = new CairoShapeInfo<UVCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Rect__MeshTileCell()
		{
			var grid = new RectGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new MeshTileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<RectPoint>(new IntRect(), p => true);
			var shapeInfo = new RectShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(RectPoint.Zero) != null;
		}
		public static bool __CompilerHint__Diamond__MeshTileCell()
		{
			var grid = new DiamondGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new MeshTileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<DiamondPoint>(new IntRect(), p => true);
			var shapeInfo = new DiamondShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(DiamondPoint.Zero) != null;
		}
		public static bool __CompilerHint__PointyHex__MeshTileCell()
		{
			var grid = new PointyHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new MeshTileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyHexPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyHexShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(PointyHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatHex__MeshTileCell()
		{
			var grid = new FlatHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new MeshTileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatHexPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatHexShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(FlatHexPoint.Zero) != null;
		}
		public static bool __CompilerHint__Line__MeshTileCell()
		{
			var grid = new LineGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid) { grid[point] = new MeshTileCell[1]; } 

			var shapeStorageInfo = new ShapeStorageInfo<LinePoint>(new IntRect(), p => true);
			var shapeInfo = new LineShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid[grid.First()][0] == null || shapeInfo.Translate(LinePoint.Zero) != null;
		}
		public static bool __CompilerHint__FlatTri__MeshTileCell()
		{
			var grid1 = new PointyHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new MeshTileCell[1]; } 

			var grid2 = new FlatTriGrid<MeshTileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatTriPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatTriShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyTri__MeshTileCell()
		{
			var grid1 = new FlatHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new MeshTileCell[1]; } 

			var grid2 = new PointyTriGrid<MeshTileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyTriPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyTriShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__FlatRhomb__MeshTileCell()
		{
			var grid1 = new FlatHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new MeshTileCell[1]; } 

			var grid2 = new FlatRhombGrid<MeshTileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<FlatRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new FlatRhombShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__PointyRhomb__MeshTileCell()
		{
			var grid1 = new PointyHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new MeshTileCell[1]; } 

			var grid2 = new PointyRhombGrid<MeshTileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<PointyRhombPoint>(new IntRect(), p => true);
			var shapeInfo = new PointyRhombShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool __CompilerHint__Cairo__MeshTileCell()
		{
			var grid1 = new PointyHexGrid<MeshTileCell[]>(1, 1);

			foreach(var point in grid1)	{ grid1[point] = new MeshTileCell[1]; } 

			var grid2 = new CairoGrid<MeshTileCell>(1, 1);

			foreach(var point in grid2)	{ grid2[point] = null; } 

			var shapeStorageInfo = new ShapeStorageInfo<CairoPoint>(new IntRect(), p => true);
			var shapeInfo = new CairoShapeInfo<MeshTileCell>(shapeStorageInfo);

			return grid1[grid1.First()][0] == null || grid2[grid2.First()] == null || shapeInfo.IncIndex(0) != null;
		}
		public static bool CallAll__()
		{
			if(!__CompilerHint__Rect__TileCell()) return false;
			if(!__CompilerHint__Diamond__TileCell()) return false;
			if(!__CompilerHint__PointyHex__TileCell()) return false;
			if(!__CompilerHint__FlatHex__TileCell()) return false;
			if(!__CompilerHint__Line__TileCell()) return false;
			if(!__CompilerHint__FlatTri__TileCell()) return false;
			if(!__CompilerHint__PointyTri__TileCell()) return false;
			if(!__CompilerHint__FlatRhomb__TileCell()) return false;
			if(!__CompilerHint__PointyRhomb__TileCell()) return false;
			if(!__CompilerHint__Cairo__TileCell()) return false;
			if(!__CompilerHint__Rect__SpriteCell()) return false;
			if(!__CompilerHint__Diamond__SpriteCell()) return false;
			if(!__CompilerHint__PointyHex__SpriteCell()) return false;
			if(!__CompilerHint__FlatHex__SpriteCell()) return false;
			if(!__CompilerHint__Line__SpriteCell()) return false;
			if(!__CompilerHint__FlatTri__SpriteCell()) return false;
			if(!__CompilerHint__PointyTri__SpriteCell()) return false;
			if(!__CompilerHint__FlatRhomb__SpriteCell()) return false;
			if(!__CompilerHint__PointyRhomb__SpriteCell()) return false;
			if(!__CompilerHint__Cairo__SpriteCell()) return false;
			if(!__CompilerHint__Rect__UVCell()) return false;
			if(!__CompilerHint__Diamond__UVCell()) return false;
			if(!__CompilerHint__PointyHex__UVCell()) return false;
			if(!__CompilerHint__FlatHex__UVCell()) return false;
			if(!__CompilerHint__Line__UVCell()) return false;
			if(!__CompilerHint__FlatTri__UVCell()) return false;
			if(!__CompilerHint__PointyTri__UVCell()) return false;
			if(!__CompilerHint__FlatRhomb__UVCell()) return false;
			if(!__CompilerHint__PointyRhomb__UVCell()) return false;
			if(!__CompilerHint__Cairo__UVCell()) return false;
			if(!__CompilerHint__Rect__MeshTileCell()) return false;
			if(!__CompilerHint__Diamond__MeshTileCell()) return false;
			if(!__CompilerHint__PointyHex__MeshTileCell()) return false;
			if(!__CompilerHint__FlatHex__MeshTileCell()) return false;
			if(!__CompilerHint__Line__MeshTileCell()) return false;
			if(!__CompilerHint__FlatTri__MeshTileCell()) return false;
			if(!__CompilerHint__PointyTri__MeshTileCell()) return false;
			if(!__CompilerHint__FlatRhomb__MeshTileCell()) return false;
			if(!__CompilerHint__PointyRhomb__MeshTileCell()) return false;
			if(!__CompilerHint__Cairo__MeshTileCell()) return false;
			return true;

		}
	}
}