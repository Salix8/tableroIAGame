using System.Collections.Generic;
using Godot;

namespace Game;

public partial class GridState : Node
{


	public enum TerrainType {
		Plains,
		Forest,
		Mountain,
		Water
	}

	[Export] HexGrid grid;

	Dictionary<Vector2I, TerrainType> terrainMap = new();

	static int GetTerrainCost(TerrainType terrain)
	{
		return terrain switch
		{
			TerrainType.Plains => 1,
			TerrainType.Forest => 2,
			TerrainType.Mountain => 3,
			TerrainType.Water => int.MaxValue,
			_ => 1
		};
	}
	public HexGrid Grid => grid;
	public int GetMovementCost(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}

	public IEnumerable<Vector2I> GetCoords()
	{
		throw new System.NotImplementedException();
	}
	public TerrainType GetTerrainType(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}
	public bool IsOccupied(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}


}