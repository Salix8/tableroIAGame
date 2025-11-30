using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct MoveTroopAction(Vector2I troopCoord, Vector2I targetCoord) : IGameAction
{

	public async Task<bool> TryApply(PlayerState playerState,WorldState worldState)
	{
		return await playerState.TryMoveTroop(troopCoord, targetCoord);
	}
}