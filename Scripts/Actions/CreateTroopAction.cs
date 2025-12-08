using System.Threading.Tasks;
using Godot;

namespace Game.State;

public readonly struct CreateTroopAction(TroopData troopData, Vector2I position, PlayerId owner) : IGameAction
{

	public async Task<bool> TryApply(WorldState worldState)
	{
		return await worldState.TrySpawnTroop(troopData, position, owner);
	}
}
