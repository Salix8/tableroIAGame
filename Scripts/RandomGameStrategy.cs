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
		
		GD.Print($"--- TURNO IA (Jugador {playerIndex}) ---"); // Debug

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
					
					// float score = mapManager.GetTileScore(neighbor); // Usamos mapa
					// Si falla el mapa, prueba con esta linea temporal: float score = 1f; 
					float score = mapManager.GetTileScore(neighbor);

					// DEBUG: Ver qué piensa la IA
					GD.Print($"Evaluando mover tropa en {troop.Position} hacia {neighbor}. Puntuación: {score}");

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
				GD.Print($"¡DECISIÓN! Mover a {bestMove} con puntuación {bestScore}"); // Debug
				return Task.FromResult<IGameAction>(new MoveTroopAction(bestTroop.Position, bestMove));
			}
		}

		// Spawn logic
		var spawns = playerState.GetSpawnableCoords().ToArray();
		if (spawns.Length == 0){
			return Task.FromResult<IGameAction>(new EmptyAction());
		}

		Vector2I bestSpawnPos = spawns[0];
		float bestSpawnScore = float.MinValue;

		foreach (var spawn in spawns)
		{
			float score = mapManager.GetTileScore(spawn);
			// GD.Print($"Evaluando spawn en {spawn}. Puntuación: {score}"); // Debug opcional

			if (score > bestSpawnScore)
			{
				bestSpawnScore = score;
				bestSpawnPos = spawn;
			}
		}
		
		GD.Print($"¡DECISIÓN! Spawnear en {bestSpawnPos} con puntuación {bestSpawnScore}");
		return Task.FromResult<IGameAction>(new CreateTroopAction(troopToSpawn, bestSpawnPos));
	}
}
