using System.Threading;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game;

public partial class TroopDataButton : Button
{
	[Export] TroopData troopToSpawn;

	public Task<TroopData> WaitForSelection(CancellationToken token)
	{

		TaskCompletionSource<TroopData> source = new();
		ButtonUp += ClickHandler;
		token.Register(() => {
			ButtonUp -= ClickHandler;
		});

		return source.Task;

		void ClickHandler()
		{
			source.SetResult(troopToSpawn);
		}
	}

	public TroopData GetRelatedTroop()
	{
		return troopToSpawn;
	}

}