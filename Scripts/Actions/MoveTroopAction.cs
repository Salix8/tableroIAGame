using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct MoveTroopAction(TroopManager.IReadonlyTroopInfo target, Vector2I[] path) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{
		var movingTroop = target;
		foreach (Vector2I coord in path){
			bool moved = await worldState.TryMoveTroopToCell(movingTroop, coord);
			if (target.CurrentHealth <= 0){
				await worldState.KillDeadTroops();
				return false;
			}
			if (!moved){
				return false;
			}
		}

		return true;
	}
}