using System.Threading.Tasks;

namespace Game.State;

public class EmptyAction : IGameAction
{
	public Task<bool> TryApply(PlayerState playerState, WorldState worldState)
	{
		return Task.FromResult(false);
	}
}