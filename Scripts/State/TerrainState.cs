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
		WizardTower,
		ManaPool,
	}

	readonly Dictionary<Vector2I, TerrainType> terrainMap = new();

	static int GetTerrainCost(TerrainType terrain)
	{
		return terrain switch
		{
			TerrainType.Mountain => int.MaxValue,
			TerrainType.Water => 2,
			_ => 1
		};
	}

	public Task AddCellAt(Vector2I position, TerrainType type)
	{
		terrainMap[position] = type;
		return cellAdded.DispatchSequential((position, type));
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
	public int? GetMovementCostToEnter(Vector2I coords)
	{
		TerrainType? type = GetTerrainType(coords);
		if (type == null){
			return null;
		}
		return GetTerrainCost(type.Value);
	}
	public TerrainType? GetTerrainType(Vector2I coord)
	{
		if (terrainMap.TryGetValue(coord, out TerrainType type)){
			return type;
		}

		return null;
	}


}
