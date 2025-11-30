using System.Threading.Tasks;

namespace Game.State;

public interface IGameAction
{
	Task<bool> TryApply(PlayerState playerState, WorldState worldState);

}