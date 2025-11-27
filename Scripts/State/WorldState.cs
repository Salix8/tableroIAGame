using Godot;
using Game;
using System.Collections.Generic;

namespace Game.State;

public class WorldState
{
	public WorldState()
	{
		terrainState = new TerrainState();
		players = new (IGameStrategy strategy, PlayerState state)[] {
			(null, new PlayerState(0)),
			(null, new PlayerState(1))
		};
	}
	readonly TerrainState terrainState;
	public TerrainState TerrainState => terrainState;
	public Dictionary<Vector2I, ManaWellState> ManaWells { get; } = new();
	public Dictionary<Vector2I, int> ConqueringClaims { get; } = new();
	public (IGameStrategy strategy, PlayerState state)[] players;

	public PlayerState GetPlayerState(int playerIndex)
	{
		return players[playerIndex].state;
	}

	public void AddTroop(int playerIndex, Vector2I coords, Troop troop)
	{
		players[playerIndex].state.Troops[coords] = troop;
	}

	public void RemoveTroop(int playerIndex, Vector2I coords)
	{
		players[playerIndex].state.Troops.Remove(coords);
	}

	public Vector2I[] GetVisibleCoords(int playerIndex)
	{
		throw new System.NotImplementedException();
	}

	public bool IsOccupied(Vector2I coords)
	{
		foreach (var playerEntry in players)
		{
			if (playerEntry.state.Troops.ContainsKey(coords))
			{
				return true;
			}
		}
		return false;
	}

	public int? GetPlayerIndexOccupying(Vector2I coords)
	{
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].state.Troops.ContainsKey(coords))
			{
				return i;
			}
		}
		return null;
	}
}