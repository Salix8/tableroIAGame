using Game;
using Game.State;
using Godot;

public partial class MapGenerator : Node3D
{
	[Export] PackedScene hexTileScene;

	[Export] HexGrid3D grid;

	[Export] public int MapRadius { get; private set; } = 5;

	private PlayableWorldState worldState;

	public override void _Ready()
	{
		worldState = GetNode<PlayableWorldState>("/root/PlayableWorldState");
		worldState.State.TerrainState.CellAdded.Subscribe(OnCellAdded);

		if (grid != null)
		{
			grid.DebugDrawRadius = MapRadius;
		}
		
		// Generate a hexagonal map
		for (int q = -MapRadius; q <= MapRadius; q++)
		{
			var r1 = Mathf.Max(-MapRadius, -q - MapRadius);
			var r2 = Mathf.Min(MapRadius, -q + MapRadius);
			for (int r = r1; r <= r2; r++)
			{
				var cell = new Vector2I(q, r);
				// Weighted random terrain selection
				TerrainState.TerrainType terrainType;
				float rand = GD.Randf(); // Random float between 0.0 and 1.0

				if (rand < 0.6f) // 60% chance for Plains
				{
					terrainType = TerrainState.TerrainType.Plains;
				}
				else if (rand < 0.8f) // 20% chance for Forest (0.6 to 0.8)
				{
					terrainType = TerrainState.TerrainType.Forest;
				}
				else if (rand < 0.9f) // 10% chance for Mountain (0.8 to 0.9)
				{
					terrainType = TerrainState.TerrainType.Mountain;
				}
				else // 10% chance for Water (0.9 to 1.0)
				{
					terrainType = TerrainState.TerrainType.Water;
				}
				worldState.State.TerrainState.AddCellAt(cell, terrainType);
			}
		}	}

	private async System.Threading.Tasks.Task OnCellAdded((Vector2I, TerrainState.TerrainType) cellData)
	{
		var (cellCoords, terrainType) = cellData;
		var tile = hexTileScene.Instantiate<HexTile>();
		AddChild(tile);
		tile.Position = grid.HexToWorld(cellCoords);
		tile.SetTerrain(terrainType);
	}
}
