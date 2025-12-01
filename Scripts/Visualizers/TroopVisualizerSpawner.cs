using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class TroopVisualizerSpawner : Node3D
{
	[Export] PackedScene troopVisualizerScene;
	[Export] PlayableWorldState state;
	[Export] HexGrid3D grid;
	public override void _Ready()
	{
		state.State.TroopSpawned.Subscribe(VisualizeTroop);
	}

	async Task VisualizeTroop((ITroopEventsHandler, TroopManager.Troop) args)
	{
		(ITroopEventsHandler troopEventsHandler, TroopManager.Troop troop) = args;
		TroopVisualizer visualizer = troopVisualizerScene.InstantiateUnderAs<TroopVisualizer>(this);
		visualizer.Initialize(grid);
		await visualizer.Spawn(troop,troopEventsHandler);
	}
}