using Game;
using Game.State;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MapGenerator : Node3D
{
	[Export] PackedScene hexTileScene;

	[Export] HexGrid3D grid;

	[Export] public int MapRadius { get; private set; } = 5;
	[Export(PropertyHint.Range, "0,5,1")] public int SmoothingIterations { get; private set; } = 1;

	[ExportSubgroup("Terrain Weights")]
	[Export(PropertyHint.Range, "0,1,0.01")] public float MountainDensity { get; private set; } = 0.12f;
	[Export(PropertyHint.Range, "0,100,1")] public int PlainsWeight { get; private set; } = 49;
    [Export(PropertyHint.Range, "0,100,1")] public int ForestWeight { get; private set; } = 28;
    [Export(PropertyHint.Range, "0,100,1")] public int WaterWeight { get; private set; } = 23;


	private PlayableWorldState worldState;

	public override void _Ready()
	{
		worldState = GetNode<PlayableWorldState>("/root/PlayableWorldState");
		worldState.State.TerrainState.CellAdded.Subscribe(OnCellAdded);

		if (grid != null)
		{
			grid.DebugDrawRadius = MapRadius;
		}

		// Calculate terrain probability thresholds from weights
		float totalWeight = PlainsWeight + ForestWeight + WaterWeight;
		float plainsThreshold = (totalWeight > 0) ? PlainsWeight / totalWeight : 1.0f;
		float forestThreshold = (totalWeight > 0) ? plainsThreshold + (ForestWeight / totalWeight) : 1.0f;

		// Phase 1: Generate Base Map (Plains, Forest, Water)
		var initialMap = new Dictionary<Vector2I, TerrainState.TerrainType>();
		for (int q = -MapRadius; q <= MapRadius; q++)
		{
			var r1 = Mathf.Max(-MapRadius, -q - MapRadius);
			var r2 = Mathf.Min(MapRadius, -q + MapRadius);
			for (int r = r1; r <= r2; r++)
			{
				var cell = new Vector2I(q, r);
				float rand = GD.Randf();

				if (rand < plainsThreshold) initialMap[cell] = TerrainState.TerrainType.Plains;
				else if (rand < forestThreshold) initialMap[cell] = TerrainState.TerrainType.Forest;
				else initialMap[cell] = TerrainState.TerrainType.Water;
			}
		}

		// Phase 2: Smoothing
		var smoothedMap = initialMap;
		for (int i = 0; i < SmoothingIterations; i++)
		{
			smoothedMap = SmoothMap(smoothedMap);
		}
		
		// Phase 3: Sprinkle Mountains
		var finalMap = new Dictionary<Vector2I, TerrainState.TerrainType>();
		foreach (var entry in smoothedMap)
		{
			if (GD.Randf() < MountainDensity)
			{
				finalMap[entry.Key] = TerrainState.TerrainType.Mountain;
			}
			else
			{
				finalMap[entry.Key] = entry.Value;
			}
		}

		// Final Population
		foreach (var entry in finalMap)
		{
			worldState.State.TerrainState.AddCellAt(entry.Key, entry.Value);
		}
	}

	private Dictionary<Vector2I, TerrainState.TerrainType> SmoothMap(Dictionary<Vector2I, TerrainState.TerrainType> originalMap)
	{
		var newMap = new Dictionary<Vector2I, TerrainState.TerrainType>();
		var terrainTypes = System.Enum.GetValues(typeof(TerrainState.TerrainType)).Cast<TerrainState.TerrainType>();

		foreach (var cellCoords in originalMap.Keys)
		{
			var voteCounts = new Dictionary<TerrainState.TerrainType, int>();
			foreach(var type in terrainTypes) voteCounts[type] = 0;

			// Central cell gets more votes (weight = 2)
			voteCounts[originalMap[cellCoords]] += 2;

			// Neighbors get 1 vote each
			foreach (var neighborCoords in HexGrid.GetNeighborCoords(cellCoords))
			{
				if (originalMap.TryGetValue(neighborCoords, out var neighborType))
				{
					voteCounts[neighborType] += 1;
				}
			}

			// The terrain type with the most votes wins
			newMap[cellCoords] = voteCounts.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		return newMap;
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
