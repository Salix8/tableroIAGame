using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Game.UI;

[GlobalClass]
public partial class TroopSelectionButton : Button
{
	[Export] TroopData troopToSpawn;

	public Task<TroopData> WaitForSelection(CancellationToken token)
	{

		TaskCompletionSource<TroopData> source = new();
		ButtonUp += ClickHandler;
		token.Register(() => {
			Callable.From(() => {
				ButtonUp -= ClickHandler;
			}).CallDeferred();
			source.TrySetCanceled();
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