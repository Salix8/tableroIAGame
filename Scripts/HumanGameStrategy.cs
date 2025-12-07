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
	public async Task<IGameAction> GetNextAction(WorldState state, PlayerId player)
	{
		IGameAction? action = null;
		while (action == null){
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

		if (!state.IsValidSpawn(player, selection)) return null;

		return await TrySpawnTroop(selection, state, player);
	}

	async Task<IGameAction?> TrySpawnTroop(Vector2I spawnPosition, WorldState state, PlayerId player)
	{

		await Task.WhenAll(
			HighlightTroops(state.GetTroops().Values, TroopVisualizer.HighlightType.Gray),
			HighlightTiles([spawnPosition], HexTileVisualizer.HighlightType.Selected),
			HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => pos != spawnPosition), HexTileVisualizer.HighlightType.Gray)
		);

		CancellationTokenSource tokenSource = new();
		Task<TroopData?> spawnTask = GetSpawnSelection(state, player, tokenSource.Token);
		Task<Vector2I> cancelTask = tileClickHandler.WaitForTileClick(tokenSource.Token);
		await Task.WhenAny([spawnTask, cancelTask]);
		await tokenSource.CancelAsync();
		await Task.WhenAll(
			HighlightTroops(state.GetTroops().Values, TroopVisualizer.HighlightType.None),
			HighlightTiles(state.TerrainState.GetFilledPositions(), HexTileVisualizer.HighlightType.None)
		);
		if (cancelTask.IsCompleted) return null;
		if (spawnTask.Result == null) return null;
		TroopData troopToSpawn = spawnTask.Result;



		return new CreateTroopAction(troopToSpawn, spawnPosition, player);
	}

	Task<TroopData?> GetSpawnSelection(WorldState state, PlayerId player, CancellationToken cancellationToken)
	{
		PlayerResources? resources = state.GetPlayerResources(player);
		Debug.Assert(resources != null, $"Player resources for player {player} not found in world state");
		return gameInterface.GetTroopSpawnSelection(resources, cancellationToken);
	}

	async Task<IGameAction?> TryMoveTroop(TroopManager.TroopInfo troop, WorldState state, PlayerId player)
	{
		troopVisualizerManager.TryGetVisualizer(troop.Position, out TroopVisualizer? visualizer);
		foreach (Vector2I filledPosition in state.TerrainState.GetFilledPositions()){
			// var tileVisualizer
		}

		Debug.Assert(visualizer != null, "No visualizer found for troop.");
		HashSet<Vector2I> reachablePositions = HexGridNavigation.ComputeReachablePositions(troop, state);
		await Task.WhenAll(
			HighlightTroops([troop], TroopVisualizer.HighlightType.Selected),
			HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => !reachablePositions.Contains(pos)), HexTileVisualizer.HighlightType.Gray)
			// HighlightTiles(state.TerrainState.GetFilledPositions().Where(pos => reachablePositions.Contains(pos)), HexTileVisualizer.HighlightType.None)
		);


		// get all reachable targets
		Vector2I selection = await tileClickHandler.WaitForTileClick(CancellationToken.None);

		await Task.WhenAll(
			HighlightTroops([troop], TroopVisualizer.HighlightType.None),
			HighlightTiles(state.TerrainState.GetFilledPositions(), HexTileVisualizer.HighlightType.None)
		);
		if (!reachablePositions.Contains(selection)){
			return null;
		}

		if (!state.IsValidTroopCoord(selection)){
			return null;
		}

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
		IList<Vector2I>? path = HexGridNavigation.ComputeOptimalPath(troop, selection, state);
		if (path == null) return null;

		return new MoveTroopAction(troop, path.ToArray());
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