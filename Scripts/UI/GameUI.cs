#nullable enable
using System;
using Godot;
using Game.State;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Visualizers;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
	[Export] PlayerStatsUI playerStatsUI;
	[Export] Button nextTurnButton;
	[Export] Label turnInfoLabel;
	[Export] TroopSelectionMenu troopSelectionMenu;

	public override void _Ready()
	{
	}

	public async Task ShowTurnText(string text)
	{
		turnInfoLabel.Text = text;
	}

	//todo work all the ui interaction methods in here as async methods
	public async Task UpdatePlayerResources((PlayerResources, PlayerResources ) args)
	{
		(PlayerResources before, PlayerResources after) = args;
		var manaTween = GetTree().CreateTween();
		manaTween.TweenMethod(Callable.From((int mana) => playerStatsUI.UpdateMana(mana)),
			before.Mana,
			after.Mana,
			Mathf.Abs(after.Mana - before.Mana) * 0.05f);
		await ToSignal(manaTween, Tween.SignalName.Finished);
	}

	public async Task<TroopData?> GetTroopSpawnSelection(PlayerResources availableResources, CancellationToken token)
	{
		troopSelectionMenu.SetEnabledTroops(troopData => troopData.Cost <= availableResources.Mana);
		await troopSelectionMenu.ShowMenu();
		try{
			TroopData? troop = await troopSelectionMenu.GetSelection(token);
			return troop;
		}
		finally{
			await troopSelectionMenu.HideMenu();
		}
	}
}