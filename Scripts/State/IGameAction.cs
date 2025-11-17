namespace Game.State;

public interface IGameAction
{
	bool TryApply(PlayerState state);

}