#nullable enable
using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct ClaimManaAction(Vector2I cell, PlayerId player) : IGameAction
{
	public async Task<bool> TryApply(WorldState worldState)
	{
		return await worldState.TryClaimManaPool(player, cell);
	}
}