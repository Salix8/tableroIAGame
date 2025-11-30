using System.Threading.Tasks;
using Game.State;

namespace Game;

public class HumanGameStrategy : IGameStrategy
{
	//todo implement human player specific logic

	public Task<IGameAction> GetNextAction(WorldState state, int playerIndex)
	{
		throw new System.NotImplementedException();
	}
}
