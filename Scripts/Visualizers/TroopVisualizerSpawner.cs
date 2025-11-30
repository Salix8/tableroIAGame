using System.Threading.Tasks;
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
		foreach (PlayerState playerState in state.State.PlayerStates){
			playerState.TroopSpawned.Subscribe(VisualizeTroop);
		}
	}

	async Task VisualizeTroop(Troop troop)
	{
		TroopVisualizer visualizer = troopVisualizerScene.InstantiateUnderAs<TroopVisualizer>(this);
		await visualizer.Spawn(troop,grid);
	}
}