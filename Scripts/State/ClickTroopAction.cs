using System.Threading.Tasks;

namespace Game.State;

// Represents a player's click on a specific troop.
public readonly struct ClickTroopAction(Troop troop) : IGameAction
{
    public Troop Troop { get; } = troop;

    // This action itself doesn't apply changes to the world state.
    // It's a signal of player intent (e.g., to select a troop).
    public Task<bool> TryApply(PlayerState playerState, WorldState worldState)
    {
        return Task.FromResult(false);
    }
}
