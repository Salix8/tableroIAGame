namespace Game;

public interface IGameAction
{
	bool TryApply(PlayerState state);

}