#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot; // Added for Vector2I
using Game.State;
using Game.UI;
using Game.Visualizers;
using Godot.Collections;
using Timer = Godot.Timer;

namespace Game;

[GlobalClass]
public partial class HumanGameStrategy : Node, IGameStrategy
{
	[Export] TileClickHandler tileClickHandler;
	[Export] HexGrid3D grid;
	[Export] GameUI gameInterface;

	[Export] TroopVisualizerManager troopVisualizerManager;
	[Export] MapVisualizer mapVisualizer;

	//
	// Dictionary<(SelectionType, SelectionType), >
	public async Task<IGameAction> GetNextAction(WorldState state, PlayerId player, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		IGameAction? action = null;
		while (action == null){
			Vector2I click = await tileClickHandler.WaitForTileClick(token);
			action = await TryCreateAction(click, state, player, token);

		}

		return action;
	}

	async Task<IGameAction?> TryCreateAction(Vector2I selection, WorldState state, PlayerId player, CancellationToken token)
	{
		if (state.TryGetTroop(selection, out TroopManager.TroopInfo? troop)){
			if (troop.Owner == player){
				return await TryMoveTroop(troop, state, player, token);
			}
		}

		if (!state.IsValidSpawn(player, selection)) return null;

		return await TrySpawnTroop(selection, state, player, token);
	}

	async Task<IGameAction?> TrySpawnTroop(Vector2I spawnPosition, WorldState state, PlayerId player, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		await Task.WhenAll(
			HighlightTroops(state.GetTroops().Values, TroopVisualizer.HighlightType.Gray),
			HighlightTiles([spawnPosition], HexTileVisualizer.HighlightType.Selected),
			HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => pos != spawnPosition), HexTileVisualizer.HighlightType.Gray)
		);

		using var spawnCts  = CancellationTokenSource.CreateLinkedTokenSource(token);
		using var clickCts  = CancellationTokenSource.CreateLinkedTokenSource(token);

		try{
			Task<TroopData?> spawnTask = GetSpawnSelection(state, player, spawnCts.Token);
			Task<Vector2I> cancelTask = tileClickHandler.WaitForTileClick(clickCts.Token);
			Task finished = await Task.WhenAny(spawnTask, cancelTask);
			if (finished == cancelTask){
				await spawnCts.CancelAsync();
				return null;
			}
			await clickCts.CancelAsync();

			TroopData? troop = await spawnTask;

			if (troop == null)
				return null;

			return new CreateTroopAction(troop, spawnPosition, player);
		}
		finally{
			await Task.WhenAll(
				HighlightTroops(state.GetTroops().Values, TroopVisualizer.HighlightType.None),
				HighlightTiles(state.TerrainState.GetFilledPositions(), HexTileVisualizer.HighlightType.None)
			);

		}
	}

	Task<TroopData?> GetSpawnSelection(WorldState state, PlayerId player, CancellationToken token)
	{
		PlayerResources? resources = state.GetPlayerResources(player);
		Debug.Assert(resources != null, $"Player resources for player {player} not found in world state");
		return gameInterface.GetTroopSpawnSelection(resources, token);
	}

	async Task<IGameAction?> TryMoveTroop(TroopManager.TroopInfo troop, WorldState state, PlayerId player, CancellationToken token)
	{

		troopVisualizerManager.TryGetVisualizer(troop.Position, out TroopVisualizer? visualizer);
		Debug.Assert(visualizer != null, "No visualizer found for troop.");
		HashSet<Vector2I> reachablePositions = HexGridNavigation.ComputeReachablePositions(troop, state);
		await Task.WhenAll(
			HighlightTroops([troop], TroopVisualizer.HighlightType.Selected),
			HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => !reachablePositions.Contains(pos)), HexTileVisualizer.HighlightType.Gray)
			// HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => reachablePositions.Contains(pos)), HexTileVisualizer.HighlightType.None)
		);

		try{
			Vector2I selection = await tileClickHandler.WaitForTileClick(token);
			if (!reachablePositions.Contains(selection)){
				return null;
			}

			if (!state.IsValidTroopCoord(selection)){
				return null;
			}

			IList<Vector2I>? path = HexGridNavigation.ComputeOptimalPath(troop, selection, state);
			if (path == null) return null;

			return new MoveTroopAction(troop, path.ToArray());
		}
		finally{

			await Task.WhenAll(
				HighlightTroops([troop], TroopVisualizer.HighlightType.None),
				HighlightTiles(state.TerrainState.GetFilledPositions(), HexTileVisualizer.HighlightType.None)
			);
		}

		// get all reachable targets


		// await HighlightTiles(state.TerrainState.GetFilledPositions(), HexTileVisualizer.HighlightType.None);
		// var enemyRanges = state.ComputeTroopRanges();
		// foreach (Vector2I position in enemyRanges.Keys){
		// 	var enemies = enemyRanges[position].Where(coveringTroop => coveringTroop.Owner != troop.Owner).ToHashSet();
		// 	if (enemies.Count == 0){
		// 		enemyRanges.Remove(position);
		// 		continue;
		// 	}
		//
		// 	enemyRanges[position] = enemies;
		// }
		//
		// await HighlightTiles(enemyRanges.Keys, HexTileVisualizer.HighlightType.Gray);
		// foreach (var ( open, current) in HexGridNavigation.Test(troop,selection,state)){
		// 	foreach (HexGridNavigation.Move move in open){
		// 		DebugDraw3D.DrawArrow(grid.HexToWorld(move.From) + Vector3.Up*1,grid.HexToWorld(move.To) + Vector3.Up*1,Colors.Red, 0.4f, duration:0.5f);
		// 	}
		//
		// 	DebugDraw3D.DrawArrow(grid.HexToWorld(current.From) + Vector3.Up*1,grid.HexToWorld(current.To) + Vector3.Up*1,Colors.SpringGreen, 0.4f, duration:0.5f);
		//
		// 	await ToSignal(GetTree().CreateTimer(0.2f), Timer.SignalName.Timeout);
		// }
		// await ToSignal(GetTree().CreateTimer(1f), Timer.SignalName.Timeout);

	}

	Task HighlightTroops(IEnumerable<TroopManager.TroopInfo> troops, TroopVisualizer.HighlightType type)
	{
		return Task.WhenAll(troops.Select(troop => {
			if (troopVisualizerManager.TryGetVisualizer(troop.Position, out TroopVisualizer? visualizer)){
				return visualizer.Highlight(type);
			}

			return Task.FromResult(true);
		}));
	}

	Task HighlightTiles(IEnumerable<Vector2I> tiles, HexTileVisualizer.HighlightType type)
	{
		return Task.WhenAll(tiles.Select(pos => {
			if (mapVisualizer.TryGetVisualizer(pos, out HexTileVisualizer? visualizer)){
				return visualizer.Highlight(type);
			}

			return Task.FromResult(true);
		}));
	}
}