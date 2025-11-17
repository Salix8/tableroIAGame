using System;
using Godot;

namespace Game;

public class WorldState
{
	public WorldState()
	{
		terrainState = new TerrainState();
		players = [];
	}
	readonly TerrainState terrainState;
	public TerrainState TerrainState => terrainState;
	(IGameStrategy strategy, PlayerState state)[] players;

	public Vector2I[] GetVisibleCoords(int playerIndex)
	{
		throw new System.NotImplementedException();
	}

	public bool IsOccupied(Vector2I coords)
	{
		throw new System.NotImplementedException();
	}

}