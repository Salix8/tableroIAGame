using System;
using System.Linq;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game;

public class RandomGameStrategy(TroopData troopToSpawn) : IGameStrategy
{
	public Task<IGameAction> GetNextAction(WorldState state, PlayerId player)
	{
		TroopManager.Troop[] playerTroops = state.GetPlayerTroops(player).ToArray();
		if (playerTroops.Length > 0){
			TroopManager.Troop randomTroop = playerTroops[GD.RandRange(0, playerTroops.Length - 1)];
			Vector2I neighborPos = Random.Shared.GetItems(HexGrid.GetNeighborCoords(randomTroop.Position).Where(state.IsValidTroopCoord).ToArray(),1).First();
			return Task.FromResult<IGameAction>(new MoveTroopAction(randomTroop, neighborPos));
		}

		Vector2I[] spawns = state.GetValidPlayerSpawns(player).ToArray();
		if (spawns.Length == 0){
			return Task.FromResult<IGameAction>(new EmptyAction());
		}
		Vector2I spawnPos = Random.Shared.GetItems(spawns,1).First();
		return Task.FromResult<IGameAction>(new CreateTroopAction(troopToSpawn, spawnPos, player));

	}
}
