#nullable enable
using Game.State;

namespace Game.FSM;

public class StateMachine(State defaultState)
{
	public State CurrentState { get; private set; } = defaultState;

	public void Set(State state)
	{
		CurrentState?.Exit();
		CurrentState = state;
		CurrentState.Enter();
	}

	public IGameAction? Poll()
	{
		return CurrentState.Poll();
	}
}