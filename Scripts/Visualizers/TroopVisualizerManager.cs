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
	[Export] PlayableWorldState state;
	[Export] HexGrid3D grid;
	Dictionary<Vector2I, TroopVisualizer> visualizers = new();
	public override void _Ready()
	{
		state.State.TroopSpawned.Subscribe(SpawnVisualizer);
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

	async Task TroopAttacking((TroopManager.TroopInfo,Vector2I) args)
	{
		(TroopManager.TroopInfo troop, Vector2I targetCoord) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(troop);
		Vector3 target = grid.HexToWorld(targetCoord);
		await visualizer.Attack(target);
	}

	async Task TroopDamaged((TroopManager.TroopInfo, TroopManager.TroopInfo) args)
	{
		(TroopManager.TroopInfo before, TroopManager.TroopInfo after) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(before);
		await visualizer.Damaged(after.CurrentHealth - before.CurrentHealth);
	}

	async Task TroopKilled(TroopManager.TroopInfo troop)
	{
		TroopVisualizer visualizer = GetTroopVisualizer(troop);
		await visualizer.Kill();
		visualizers.Remove(troop.Position);
		visualizer.QueueFree();

	}

	async Task TroopMoved((TroopManager.TroopInfo, TroopManager.TroopInfo) args)
	{
		(TroopManager.TroopInfo before, TroopManager.TroopInfo after) = args;
		TroopVisualizer visualizer = GetTroopVisualizer(before);
		Vector3 target = grid.HexToWorld(after.Position);
		await visualizer.MoveTo(target);
		visualizers.Remove(before.Position);
		Debug.Assert(!visualizers.ContainsKey(after.Position), $"Can't move visualizer to {after.Position}. Position occupied by another visualizer");
		visualizers.Add(after.Position, visualizer);
	}

	TroopVisualizer GetTroopVisualizer(TroopManager.TroopInfo relatedTroop)
	{
		Debug.Assert(visualizers.ContainsKey(relatedTroop.Position), $"Visualizer doesn't exist for related troop at {relatedTroop.Position}");
		return visualizers[relatedTroop.Position];
	}

	async Task SpawnVisualizer((ITroopEventsHandler, TroopManager.TroopInfo) args)
	{
		(ITroopEventsHandler troopEventsHandler, TroopManager.TroopInfo troop) = args;
		troopEventsHandler.GetTroopMovedHandler().Subscribe(TroopMoved);
		troopEventsHandler.GetTroopKilledHandler().Subscribe(TroopKilled);
		troopEventsHandler.GetTroopAttackingHandler().Subscribe(TroopAttacking);
		troopEventsHandler.GetTroopDamagedHandler().Subscribe(TroopDamaged);
		TroopVisualizer visualizer = troopVisualizerScene.InstantiateUnderAs<TroopVisualizer>(this);
		Vector3 position = grid.HexToWorld(troop.Position);
		Debug.Assert(!visualizers.ContainsKey(troop.Position), $"A visualizer already exists at coordinate {troop.Position}");
		visualizers.Add(troop.Position, visualizer);
		//todo add base material for
		await visualizer.Spawn(position,troop.Data);
	}
}