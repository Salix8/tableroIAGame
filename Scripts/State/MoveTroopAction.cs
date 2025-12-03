using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct MoveTroopAction(TroopManager.TroopInfo troop, Vector2I target) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{
		return await worldState.TryMoveTroop(troop,target);
	}
}