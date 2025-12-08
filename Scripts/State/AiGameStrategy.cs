using System.Threading;
using System.Threading.Tasks;

namespace Game.State;

public class AiGameStrategy : IGameStrategy
{
	//todo implement AI player specific logic

	public Task<IGameAction> GetNextAction(WorldState state, PlayerId player, CancellationToken token)
	{
		throw new System.NotImplementedException();
	}
}
