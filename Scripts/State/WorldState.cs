using Godot;
using Game;
using System.Collections.Generic;
using System.Linq;

namespace Game.State;

public interface IReadonlyWorldState
{
	//todo implement this to hide write stuff for gameactions
}
public class WorldState
{
	public WorldState(int playerAmount )
	{
		terrainState = new TerrainState();
		playerStates = new PlayerState[playerAmount];
		for (int i = 0; i < playerAmount; i++){
			int playerIndex = i;
			playerStates[playerIndex] = new PlayerState(playerIndex);
			playerStates[playerIndex].ClaimedCoord += (coord => {
				OnCoordClaimed(coord,playerIndex);
			});
		}
	}

	void OnCoordClaimed(Vector2I coord, int playerIndex)
	{
		for (int i = 0; i < playerStates.Length; i++){
			if (i == playerIndex){
				continue;
			}
			PlayerState state = playerStates[i];
			state.RemoveClaim(coord);
		}
	}
	readonly TerrainState terrainState;
	public TerrainState TerrainState => terrainState;
	readonly PlayerState[] playerStates;
	public IReadOnlyList<PlayerState> PlayerStates => playerStates;

	public PlayerState GetPlayerState(int playerIndex)
	{
		return playerStates[playerIndex];
	}

	public Vector2I[] GetVisibleCoords(int playerIndex)
	{
		throw new System.NotImplementedException();
	}

	public bool IsOccupied(Vector2I coords)
	{
		return playerStates.Any(playerEntry => playerEntry.TryGetTroop(coords,out _));
	}

	public int? GetPlayerIndexOccupying(Vector2I coords)
	{
		for (int i = 0; i < playerStates.Length; i++)
		{
			if (playerStates[i].TryGetTroop(coords, out _))
			{
				return i;
			}
		}
		return null;
	}
}