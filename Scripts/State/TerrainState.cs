using System.Collections.Generic;
using System.Threading.Tasks;
using Game.AsyncEvents.Generic;
using Godot;

namespace Game.State;

public class TerrainState()
{
	public enum TerrainType {
		Plains,
		Forest,
		Mountain,
		Water,
		WizardTower
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
			TerrainType.WizardTower => 1,
			_ => 1
		};
	}

	public Task AddCellAt(Vector2I position, TerrainType type)
	{
		terrainMap[position] = type;
		return cellAdded.DispatchParallel((position, type));
	}

	readonly AsyncEvent<(Vector2I, TerrainType)> cellAdded = new();
	public IAsyncHandlerCollection<(Vector2I, TerrainType)> CellAdded => cellAdded;


	public IEnumerable<Vector2I> GetFilledPositions()
	{
		return terrainMap.Keys;
	}

	public IEnumerable<Vector2I> GetTerrainCoordinates(TerrainType type)
	{
		foreach (var entry in terrainMap)
		{
			if (entry.Value == type)
			{
				yield return entry.Key;
			}
		}
	}
	public int GetMovementCostToEnter(Vector2I coords)
	{
		return GetTerrainCost(GetTerrainType(coords));
	}
	public TerrainType GetTerrainType(Vector2I coords)
	{
		if (terrainMap.TryGetValue(coords, out TerrainType value))
		{
			return value;
			}
		return TerrainType.Plains;
	}


}
