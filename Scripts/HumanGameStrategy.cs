using System;
using System.Threading.Tasks;
using Godot;
using Game.State;

namespace Game;

public class HumanGameStrategy : IGameStrategy
{
	private TaskCompletionSource<IGameAction> _nextActionCompletionSource;

    public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
    {
        _nextActionCompletionSource = new TaskCompletionSource<IGameAction>();
        return _nextActionCompletionSource.Task;
    }

    public void OnHexClicked(Vector2I hexCoord)
    {
        _nextActionCompletionSource?.TrySetResult(new ClickHexAction(hexCoord));
    }

    public void OnTroopClicked(Troop troop)
    {
        _nextActionCompletionSource?.TrySetResult(new ClickTroopAction(troop));
    }

    public void OnManaPoolClicked(Vector2I coord)
    {
        _nextActionCompletionSource?.TrySetResult(new ClickManaPoolAction(coord));
    }

    public void ProcessTroopSelection(Troop troop)
    {
        // Future logic will go here. For example, checking if the troop is friendly,
        // selecting it, and waiting for the next player input (e.g., a move command).
        throw new NotImplementedException("Logic for troop selection/action is not yet implemented.");
    }

    public void ProcessManaPoolInteraction(Vector2I coord)
    {
        // Future logic will go here. For example, checking if the mana pool is owned,
        // attempting to claim it, or receiving mana.
        throw new NotImplementedException("Logic for mana pool interaction is not yet implemented.");
    }
}
