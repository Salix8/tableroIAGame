#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Godot; // Added for Vector2I
using Game.State;
using Game.UI;
using Game.Visualizers;
using Godot.Collections;

namespace Game;

[GlobalClass]
public partial class HumanGameStrategy : Node, IGameStrategy
{

	[Export] TileClickHandler tileClickHandler;
	[Export] HexGrid3D grid;
	[Export] GameUI gameInterface;
	//
	// Dictionary<(SelectionType, SelectionType), >
	public async Task<IGameAction> GetNextAction(WorldState state, PlayerId player)
	{
		IGameAction? action = null;
		while (action != null){
			Vector2I click = await tileClickHandler.WaitForTileClick(CancellationToken.None);
			action = await TryCreateAction(click, state, player);
		}

		return action;

	}

	async Task<IGameAction?> TryCreateAction(Vector2I selection, WorldState state, PlayerId player)
	{
		if (state.TryGetTroop(selection, out TroopManager.TroopInfo? troop)){
			if (troop.Owner == player){
				return await TryMoveTroop(troop, state, player);
			}
		}

		if (state.IsValidSpawn(player, selection)){
			return await TrySpawnTroop(selection, state, player);
		}

		return null;

	}

	async Task<IGameAction?> TrySpawnTroop(Vector2I spawnPosition, WorldState state, PlayerId player)
	{
		CancellationTokenSource tokenSource = new();
		Task<TroopData> spawnTask = GetSpawnSelection(state, player,tokenSource.Token);
		Task<Vector2I> cancelTask = tileClickHandler.WaitForTileClick(tokenSource.Token);

		await Task.WhenAny([spawnTask, cancelTask]);
		await tokenSource.CancelAsync();
		if (cancelTask.IsCompleted) return null;
		TroopData troopToSpawn = spawnTask.Result;
		return new CreateTroopAction(troopToSpawn, spawnPosition, player);

	}

	Task<TroopData> GetSpawnSelection(WorldState state, PlayerId player, CancellationToken cancellationToken)
	{
		PlayerResources? resources = state.GetPlayerResources(player);
		Debug.Assert(resources != null, $"Player resources for player {player.Value} not found in world state");
		List<Task<TroopData>> selectionTasks = [];
		//gameInterface.SelectTroop
		// foreach (UI.TroopSelectionButton troopSelectionButton in gameInterface){
		// 	TroopData data = troopSelectionButton.GetRelatedTroop();
		// 	if (data.Cost <= resources.Value.Mana){
		// 		troopSelectionButton.Disabled = false;
		// 		selectionTasks.Add(troopSelectionButton.WaitForSelection(cancellationToken));
		// 	}
		// 	else{
		// 		troopSelectionButton.Disabled = true;
		// 	}
		// }

		return Task.WhenAny(selectionTasks.ToArray()).Result;
	}

	async Task<IGameAction?> TryMoveTroop(TroopManager.TroopInfo troop, WorldState state, PlayerId player)
	{
		// get all reachable targets
		Vector2I selection = await tileClickHandler.WaitForTileClick(CancellationToken.None);
		if (state.IsValidTroopCoord(selection)){
			return new MoveTroopAction(troop,selection);
		}

		return null;

	}

}