#nullable enable
using System;
using Godot;
using Game.State;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game.UI
{
	public partial class TroopSelectionMenu : Control
	{
		TroopSelectionButton[] troopButtons;

		public override void _Ready()
		{
			Visible = false;
			Position = Position with{ Y = GetWindow().Size.Y};
			troopButtons = this.GetAllChildrenOfType<TroopSelectionButton>().ToArray();
		}

		public async Task ShowMenu()
		{
			if (Visible) return;

			Visible = true;
			Tween tween = CreateTween();
			float targetY = GetWindow().Size.Y - Size.Y;
			tween.TweenProperty(this, "position:y", targetY, 0.5f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
			await ToSignal(tween, Tween.SignalName.Finished);
		}

		public void SetEnabledTroops(Func<TroopData, bool> enabledPredicate)
		{
			foreach (TroopSelectionButton troopButton in troopButtons){
				troopButton.Disabled = !enabledPredicate(troopButton.GetRelatedTroop());
			}
		}

		public async Task<TroopData?> GetSelection(CancellationToken cancellationToken)
		{
			using CancellationTokenSource buttonCancelSource = new();
			using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, buttonCancelSource.Token);
			Task<TroopData>[] selectionTasks = troopButtons
				// .Where(btn => !btn.Disabled) // commented out cuz this honestly makes more sense without relying on disabled
				.Select(btn => btn.WaitForSelection(linkedSource.Token))
				.ToArray();
			if (selectionTasks.Length == 0){
				await buttonCancelSource.CancelAsync();
				return null;
			}
			Task<TroopData> resultTask = await Task.WhenAny(selectionTasks);
			await buttonCancelSource.CancelAsync();
			if (resultTask.IsCanceled || resultTask.IsFaulted){
				return null;
			}

			return await resultTask;

		}

		public async Task HideMenu()
		{
			if (!Visible) return;

			Tween tween = CreateTween();
			float targetY = GetWindow().Size.Y;
			tween.TweenProperty(this, "position:y", targetY, 0.3f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In);
			await ToSignal(tween, Tween.SignalName.Finished);
			Visible = false;
		}
	}
}