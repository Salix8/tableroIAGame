using System;
using System.Linq;
using System.Threading.Tasks;
using Game.State;
using Game.AI; 
using Godot;

namespace Game;

public class RandomGameStrategy(TroopData troopToSpawn, InfluenceMapManager mapManager) : IGameStrategy
{
	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
	{
		mapManager.UpdateMaps(state, playerIndex);

		PlayerState playerState = state.GetPlayerState(playerIndex);
		
		GD.Print($"--- TURNO IA (Jugador {playerIndex}) ---");

		if (playerState.Troops.Count > 0)
		{
			Troop bestTroop = default;
			Vector2I bestMove = Vector2I.Zero;
			float bestScore = float.MinValue;
			bool foundMove = false;

			foreach (var troop in playerState.Troops)
			{
				foreach (var neighbor in HexGrid3D.GetNeighborCoords(troop.Position))
				{
					if (state.IsOccupied(neighbor)) continue;
					
					float score = mapManager.GetTileScore(neighbor);


					if (score > bestScore)
					{
						bestScore = score;
						bestTroop = troop;
						bestMove = neighbor;
						foundMove = true;
					}
				}
			}

			if (foundMove)
			{
				GD.Print($"IA Mueve: {bestMove} (Score: {bestScore:0.0})");
				return Task.FromResult<IGameAction>(new MoveTroopAction(bestTroop.Position, bestMove));
			}
		}
		var spawns = playerState.GetSpawnableCoords().ToArray();
		if (spawns.Length == 0){
			return Task.FromResult<IGameAction>(new EmptyAction());
		}

		Vector2I bestSpawnPos = spawns[0];
		float bestSpawnScore = float.MinValue;

		foreach (var spawn in spawns)
		{
			float score = mapManager.GetTileScore(spawn);
			if (score > bestSpawnScore)
			{
				bestSpawnScore = score;
				bestSpawnPos = spawn;
			}
		}

		GD.Print($"IA Spawnea: {bestSpawnPos} (Score: {bestSpawnScore:0.0})");
		return Task.FromResult<IGameAction>(new CreateTroopAction(troopToSpawn, bestSpawnPos));
	}
}
