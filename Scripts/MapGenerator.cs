using Game;
using Godot;

public partial class MapGenerator : Node3D
{
	[Export] PackedScene hexTileScene;

	[Export] HexGrid3D grid;

	public override void _Ready()
	{
	}
	public void GenerateMap()
	{
	}
}
