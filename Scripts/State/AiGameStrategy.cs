using System.Threading.Tasks;

namespace Game.State;

public class AiGameStrategy : IGameStrategy
{
	//todo implement AI player specific logic

	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
	{
		throw new System.NotImplementedException();
	}
}