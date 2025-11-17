using System;
using System.Collections.Generic;
using Godot;

namespace Game;

public class TerrainState()
{
	public enum TerrainType {
		Plains,
		Forest,
		Mountain,
		Water
	}

	readonly Dictionary<Vector2I, TerrainType> terrainMap = new();

	static int GetTerrainCost(TerrainType terrain)
	{
		return terrain switch
		{
			TerrainType.Plains => 1,
			TerrainType.Forest => 1,
			TerrainType.Mountain => int.MaxValue,
			TerrainType.Water => 2,
			_ => 1
		};
	}

	public void AddCellAt(Vector2I position, TerrainType type)
	{
		terrainMap[position] = type;
		CellAdded?.Invoke(position);
	}

	public event Action<Vector2I> CellAdded;

	public IEnumerable<Vector2I> GetFilledPositions()
	{
		return terrainMap.Keys;
	}
	// public HexGrid Grid => grid;
	public int GetMovementCostToEnter(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}
	public TerrainType GetTerrainType(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}


}