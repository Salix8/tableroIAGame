using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct MoveTroopAction(TroopManager.TroopInfo target, Vector2I[] path) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{
		var movingTroop = target;
		foreach (Vector2I coord in path){
			(TroopManager.TroopInfo movedTroop, bool moved) = await worldState.TryMoveTroopToCell(movingTroop, coord);
			if (movedTroop.CurrentHealth <= 0){
				await worldState.KillDeadTroops();
				return false;
			}
			if (!moved){
				return false;
			}

			movingTroop = movedTroop;
		}

		return true;
	}
}