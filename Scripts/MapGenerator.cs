using Game;
using Godot;

public partial class MapGenerator : Node3D
{
	[Export] PackedScene hexTileScene;

	[Export] HexGrid3D grid;

	public override void _Ready()
	{
		//todo: tap into the terrain state on created event to create terrain in game
	}

}
