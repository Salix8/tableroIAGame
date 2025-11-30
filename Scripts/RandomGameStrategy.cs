using System;
using System.Linq;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game;

public class RandomGameStrategy(TroopData troopToSpawn) : IGameStrategy
{
	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
	{
		PlayerState playerState = state.GetPlayerState(playerIndex);
		if (playerState.Troops.Count > 0){
			Troop randomTroop = playerState.Troops[GD.RandRange(0, playerState.Troops.Count - 1)];
			Vector2I neighborPos = Random.Shared.GetItems(HexGrid.GetNeighborCoords(randomTroop.Position).ToArray(),1).First();
			return Task.FromResult<IGameAction>(new MoveTroopAction(randomTroop.Position, neighborPos));
		}

		var spawns = playerState.GetSpawnableCoords().ToArray();
		if (spawns.Length == 0){
			return Task.FromResult<IGameAction>(new EmptyAction());
		}
		var spawnPos = Random.Shared.GetItems(playerState.GetSpawnableCoords().ToArray(),1).First();
		return Task.FromResult<IGameAction>(new CreateTroopAction(troopToSpawn, spawnPos));


	}
}
