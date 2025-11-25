using Game.State;
using Godot;

namespace Game;

[GlobalClass]
public partial class HexGrid3D : Node3D
{
	[Export] public float CellRadius { get; private set; }
	public int DebugDrawRadius { get; set; } = 5;
	HexGrid grid;
	public override void _EnterTree()
	{
		grid = new HexGrid(CellRadius);
	}

	public Vector2I WorldToHex(Vector3 worldPosition)
	{
		Vector3 local = worldPosition * GlobalTransform;
		Vector2 flat = new Vector2(local.X, local.Z);
		return grid.WorldToHex(flat);
	}

	public Vector3 HexToWorld(Vector2I hexCoord, float y = 0)
	{
		Vector2 flat = grid.HexToWorld(hexCoord);
		Vector3 local = new Vector3(flat.X, y, flat.Y);
		return GlobalTransform * local;
	}

	public void DrawCell(Vector2I cell, Color color)
	{
		Vector3 pos = HexToWorld(cell);
		DebugDraw3D.DrawSphere(pos, CellRadius, color);
	}

	public override void _Process(double delta)
	{
		DebugDraw3D.DrawArrow(Vector3.Zero, Vector3.Up, Colors.Green);
		DebugDraw3D.DrawArrow(Vector3.Zero, Vector3.Forward, Colors.Blue);
		DebugDraw3D.DrawArrow(Vector3.Zero, Vector3.Right, Colors.Red);
		// DrawCell(Vector2I.Zero, Colors.Aqua);
		// DrawCell(Vector2I.Zero + HexGrid.UpRight, Colors.Green);
		// DrawCell(Vector2I.Zero + HexGrid.Up, Colors.Red);
		// DrawCell(Vector2I.Zero + HexGrid.DownRight, Colors.Blue);
		foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighborSpiralCoords(Vector2I.Zero, DebugDrawRadius)){
			//DebugDraw2D.SetText($"Coord {neighborSpiralCoord}");
			// DrawCell(neighborSpiralCoord,Colors.Red);
		}
	}
}
