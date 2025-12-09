using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct MoveTroopAction(TroopManager.IReadonlyTroopInfo target, Vector2I[] path) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{
		worldState.LockTroop(target);
		var movingTroop = target;
		if (!worldState.GetTroops().Values.Contains(movingTroop)){
			GD.Print("AAA");
		}
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