using System.Threading.Tasks;

namespace Game.State;

public interface IGameStrategy
{
	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex);

}
