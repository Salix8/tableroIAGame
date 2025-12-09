#nullable enable
using Game.State;

namespace Game.FSM;

public abstract class State
{
	public abstract void Enter();
	public abstract IGameAction? Poll();
	public abstract void Exit();
}