namespace Game.State;

public interface IGameStrategy
{
	public IGameAction GetNextAction(WorldState state, int playerIndex);

}
