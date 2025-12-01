using System.Threading.Tasks;
using Godot; // Added for Vector2I

namespace Game.State;

// Represents a player's click on a specific mana pool coordinate.
public readonly struct ClickManaPoolAction(Vector2I hexCoord) : IGameAction
{
    public Vector2I HexCoord { get; } = hexCoord;

    // This action itself doesn't apply changes to the world state.
    // It's a signal to the HumanGameStrategy that a mana pool was clicked.
    public Task<bool> TryApply(PlayerState playerState, WorldState worldState)
    {
        return Task.FromResult(false);
    }
}
