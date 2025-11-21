using Game;
using Game.State;
using Godot;

public partial class MapGenerator : Node3D
{
	[Export] PackedScene hexTileScene;

	[Export] HexGrid3D grid;

	private PlayableWorldState worldState;

	public override void _Ready()
	{
		worldState = GetNode<PlayableWorldState>("/root/PlayableWorldState");
		worldState.State.TerrainState.CellAdded.Subscribe(OnCellAdded);

		// Generate a simple map
		for (int q = -5; q <= 5; q++)
		{
			for (int r = -5; r <= 5; r++)
			{
				if (q * q + r * r > 25) continue;

				var cell = new Vector2I(q, r);
				var terrainType = (TerrainState.TerrainType)GD.RandRange(0, 3);
				worldState.State.TerrainState.AddCellAt(cell, terrainType);
			}
		}
	}

	private async System.Threading.Tasks.Task OnCellAdded((Vector2I, TerrainState.TerrainType) cellData)
	{
		var (cellCoords, terrainType) = cellData;
		var tile = hexTileScene.Instantiate<HexTile>();
		AddChild(tile);
		tile.Position = grid.HexToWorld(cellCoords);
		tile.SetTerrain(terrainType);
	}
}
