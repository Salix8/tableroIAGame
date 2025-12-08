using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game;

public class RandomGameStrategy(TroopData troopToSpawn) : IGameStrategy
{

	public async IAsyncEnumerable<IGameAction> GetActionGenerator(WorldState state, PlayerId player, int desiredActions, [EnumeratorCancellation] CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		for (int i = 0; i < desiredActions; i++){
			TroopManager.TroopInfo[] playerTroops = state.GetPlayerTroops(player).ToArray();
			if (playerTroops.Length > 0){
				TroopManager.TroopInfo randomTroop = playerTroops[GD.RandRange(0, playerTroops.Length - 1)];
				Vector2I neighborPos = Random.Shared.GetItems(HexGrid.GetNeighborCoords(randomTroop.Position).Where(state.IsValidTroopCoord).ToArray(),1).First();
				yield return new MoveTroopAction(randomTroop, [neighborPos]);
				continue;
			}

			Vector2I[] spawns = state.GetValidPlayerSpawns(player).ToArray();
			if (spawns.Length == 0){
				yield return new EmptyAction();
				continue;
			}
			Vector2I spawnPos = Random.Shared.GetItems(spawns,1).First();
			yield return new CreateTroopAction(troopToSpawn, spawnPos, player);
			continue;
		}
	}
}
