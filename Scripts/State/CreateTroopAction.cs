using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct CreateTroopAction(TroopData troopData, Vector2I position) : IGameAction
{

	public async Task<bool> TryApply(PlayerState playerState,WorldState worldState)
	{
		if (!playerState.IsSpawnableCoord(position)){
			return false;
		}

		if (worldState.IsOccupied(position)){
			return false;
		}

		if (worldState.TerrainState.GetTerrainType(position) is TerrainState.TerrainType.Mountain or null){
			return false;
		}

		return await playerState.TrySpawnTroop(troopData, position);

	}
}
