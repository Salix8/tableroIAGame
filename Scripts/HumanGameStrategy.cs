using System.Threading.Tasks;
using Godot; // Added for Vector2I
using Game.State;

namespace Game;

public class HumanGameStrategy : IGameStrategy
{
	private TaskCompletionSource<IGameAction> _nextActionCompletionSource;

	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
	{
		// Ensure a new TaskCompletionSource is ready for the next player action
		_nextActionCompletionSource = new TaskCompletionSource<IGameAction>();
		return _nextActionCompletionSource.Task;
	}

	// This method will be called by PlayableWorldState when a human player clicks a hex
	public void OnHexClicked(Vector2I hexCoord)
	{
		// Complete the task with a ClickAction, which PlayableWorldState will then process
		_nextActionCompletionSource?.TrySetResult(new ClickAction(hexCoord));
	}
}
