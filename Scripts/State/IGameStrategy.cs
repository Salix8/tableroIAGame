using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Game.State;

public interface IGameStrategy
{
	public IAsyncEnumerable<IGameAction> GetActionGenerator(WorldState state, PlayerId player, int desiredActions, CancellationToken token);

}
