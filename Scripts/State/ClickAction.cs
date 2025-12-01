using System.Threading.Tasks;
using Godot;

namespace Game.State;

// Represents a player's click on a specific hex coordinate.
public readonly struct ClickAction(Vector2I hexCoord) : IGameAction
{
    public Vector2I HexCoord { get; } = hexCoord;

    // For now, a ClickAction doesn't modify the game state directly.
    // It's a signal to the HumanGameStrategy that a click occurred.
    // The strategy will then interpret this click to form a "real" action (e.g., MoveTroopAction).
    public Task<bool> TryApply(PlayerState playerState, WorldState worldState)
    {
        // This action itself doesn't apply changes to the world state.
        // It's just a signal of player intent.
        return Task.FromResult(false);
    }
}
