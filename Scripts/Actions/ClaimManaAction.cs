#nullable enable
using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct ClaimManaAction(TroopManager.IReadonlyTroopInfo troop, PlayerId player) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{

		worldState.LockTroop(troop);
		return await worldState.TryClaimManaPool(player, troop.Position);
	}
}