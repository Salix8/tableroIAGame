#nullable enable
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Game.State;
using Godot;
using Godot.Collections;

namespace Game.Visualizers;

[GlobalClass]
public partial class TroopVisualizerManager : Node3D
{
	[Export] PackedScene troopVisualizerScene;
	[Export] PlayableMatch game;
	[Export] HexGrid3D grid;
	Dictionary<Vector2I, TroopVisualizer> visualizers = new();

	public override void _Ready()
	{
		game.State.TroopSpawned.Subscribe(SpawnVisualizer);
	}

	public bool TryGetVisualizer(Vector2I coord, [NotNullWhen(true)] out TroopVisualizer? visualizer)
	{
		if (visualizers.TryGetValue(coord, out TroopVisualizer? vis)){
			visualizer = vis;
			return true;
		}

		visualizer = null;
		return false;
	}

	async Task TroopAttacking((TroopManager.IReadonlyTroopInfo, Vector2I) args)
	{
		(TroopManager.IReadonlyTroopInfo troop, Vector2I targetCoord) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(troop.Position);
		Vector3 target = grid.HexToWorld(targetCoord);
		await visualizer.Attack(target);
	}

	async Task TroopDamaged((TroopManager.TroopSnapshot, TroopManager.IReadonlyTroopInfo) args)
	{
		(TroopManager.TroopSnapshot before, TroopManager.IReadonlyTroopInfo after) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(before.Position);
		await visualizer.Damaged(after.CurrentHealth - before.CurrentHealth);
	}

	async Task TroopKilled(TroopManager.IReadonlyTroopInfo troop)
	{
		TroopVisualizer visualizer = GetTroopVisualizer(troop.Position);
		await visualizer.Kill();
		visualizers.Remove(troop.Position);
		visualizer.QueueFree();
	}

	async Task TroopMoved((TroopManager.TroopSnapshot, TroopManager.IReadonlyTroopInfo) args)
	{
		(TroopManager.TroopSnapshot before, TroopManager.IReadonlyTroopInfo current) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(before.Position);
		Vector3 target = grid.HexToWorld(current.Position);
		await visualizer.MoveTo(target);
		visualizers.Remove(before.Position);
		Debug.Assert(!visualizers.ContainsKey(current.Position),
			$"Can't move visualizer to {current.Position}. Position occupied by another visualizer");
		visualizers.Add(current.Position, visualizer);
	}

	TroopVisualizer GetTroopVisualizer(Vector2I position)
	{
		Debug.Assert(visualizers.ContainsKey(position),
			$"Visualizer doesn't exist at {position}");
		return visualizers[position];
	}

	async Task SpawnVisualizer((ITroopEventsHandler, TroopManager.IReadonlyTroopInfo) args)
	{
		(ITroopEventsHandler troopEventsHandler, TroopManager.IReadonlyTroopInfo troop) = args;
		troopEventsHandler.GetTroopMovedHandler().Subscribe(TroopMoved);
		troopEventsHandler.GetTroopKilledHandler().Subscribe(TroopKilled);
		troopEventsHandler.GetTroopAttackingHandler().Subscribe(TroopAttacking);
		troopEventsHandler.GetTroopDamagedHandler().Subscribe(TroopDamaged);
		TroopVisualizer visualizer = troopVisualizerScene.InstantiateUnderAs<TroopVisualizer>(this);
		game.Players.TryGetValue(troop.Owner, out PlayerInfo? info);
		Debug.Assert(info != null, "Spawned troop does not belong to a valid player");
		Vector3 position = grid.HexToWorld(troop.Position);
		Debug.Assert(!visualizers.ContainsKey(troop.Position),
			$"A visualizer already exists at coordinate {troop.Position}");
		visualizers.Add(troop.Position, visualizer);
		await visualizer.Spawn(position, troop.Data, info.TroopColor);
	}
}