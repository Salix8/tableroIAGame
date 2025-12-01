using System;
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Godot;

namespace Game;

public partial class MapGenerator : Node3D
{
	// [Export] PlayableWorldState worldState;

	[Export] public int MapRadius { get; private set; } = 5;
	[Export(PropertyHint.Range, "0,5,1")] public int SmoothingIterations { get; private set; } = 1;

	[ExportSubgroup("Terrain Weights")]
	[Export(PropertyHint.Range, "0,1,0.01")] public float MountainDensity { get; private set; } = 0.12f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float WizardTowerDensity { get; private set; } = 0.04f;
	[Export(PropertyHint.Range, "0,100,1")] public int PlainsWeight { get; private set; } = 49;
	[Export(PropertyHint.Range, "0,100,1")] public int ForestWeight { get; private set; } = 28;
	[Export(PropertyHint.Range, "0,100,1")] public int WaterWeight { get; private set; } = 23;

	public override void _Ready()
	{


	}

	public (Dictionary<Vector2I, TerrainState.TerrainType> map, Vector2I mana1Pos, Vector2I mana2Pos) GenerateMap()
	{

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
		var mountainMap = new Dictionary<Vector2I, TerrainState.TerrainType>();
		foreach (var entry in smoothedMap)
		{
			if (GD.Randf() < MountainDensity)
			{
				mountainMap[entry.Key] = TerrainState.TerrainType.Mountain;
			}
			else
			{
				mountainMap[entry.Key] = entry.Value;
			}
		}

		// Phase 4: Place Mana Wells
		var finalMap = new Dictionary<Vector2I, TerrainState.TerrainType>(mountainMap); // Copy mountainMap to finalMap
		var startingWellCoords = new HashSet<Vector2I>();

		// --- Place two guaranteed starting wells ---
		List<Vector2I> availablePlains = mountainMap
			.Where(entry => entry.Value == TerrainState.TerrainType.Plains)
			.Select(entry => entry.Key)
			.ToList();

		if (availablePlains.Count < 2)
		{
			GD.PrintErr("Not enough plains tiles to place starting Mana Wells! Using fallback.");
			availablePlains = mountainMap
				.Where(entry => entry.Value != TerrainState.TerrainType.Mountain && entry.Value != TerrainState.TerrainType.Water)
				.Select(entry => entry.Key)
				.ToList();
		}

		GD.Randomize();

		// Select first Mana Well for Player 0
		int randomIndex1 = (int)(GD.Randi() % availablePlains.Count);
		Vector2I well1Coords = availablePlains[randomIndex1];
		finalMap[well1Coords] = TerrainState.TerrainType.ManaPool;
		// worldState.State.ManaWells.Add(well1Coords, new ManaWellState { OwnerIndex = 0 });
		startingWellCoords.Add(well1Coords);
		availablePlains.RemoveAt(randomIndex1);

		// Select second Mana Well for Player 1
		int randomIndex2 = (int)(GD.Randi() % availablePlains.Count);
		Vector2I well2Coords = availablePlains[randomIndex2];
		finalMap[well2Coords] = TerrainState.TerrainType.ManaPool;
		// worldState.State.ManaWells.Add(well2Coords, new ManaWellState { OwnerIndex = 1 });
		startingWellCoords.Add(well2Coords);

		// --- Sprinkle neutral wells ---
		foreach (var entry in mountainMap)
		{
			// Skip if it's already a starting well, or if it's a mountain/water tile
			if (startingWellCoords.Contains(entry.Key) || entry.Value == TerrainState.TerrainType.Mountain || entry.Value == TerrainState.TerrainType.Water)
			{
				continue;
			}

			if (GD.Randf() < WizardTowerDensity)
			{
				finalMap[entry.Key] = TerrainState.TerrainType.WizardTower;
				// worldState.State.ManaWells.Add(entry.Key, new ManaWellState { OwnerIndex = null });
			}
		}

		// Final Population
		return (finalMap,well1Coords,well2Coords);
	}

	static Dictionary<Vector2I, TerrainState.TerrainType> SmoothMap(Dictionary<Vector2I, TerrainState.TerrainType> originalMap)
	{
		var newMap = new Dictionary<Vector2I, TerrainState.TerrainType>();
		var terrainTypes = Enum.GetValues(typeof(TerrainState.TerrainType));

		foreach (Vector2I cellCoords in originalMap.Keys)
		{
			var voteCounts = new Dictionary<TerrainState.TerrainType, int>();
			foreach(TerrainState.TerrainType type in terrainTypes) voteCounts[type] = 0;

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

}
