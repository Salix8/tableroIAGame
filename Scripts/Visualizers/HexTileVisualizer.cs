using System.Threading.Tasks;
using Game.State;
using Godot;
using Godot.Collections;

namespace Game.Visualizers;

public partial class HexTileVisualizer : Node3D
{
	[Export] Dictionary<TerrainState.TerrainType, PackedScene> terrainScenes;

	[Export] float totalAnimationDuration = 0.5f;

	[Export] float skippedDuration = 0.2f;
	// [Export] public Node3D Plains { get; set; }
	// [Export] public Node3D Forest { get; set; }
	// [Export] public Node3D Mountain { get; set; }
	// [Export] public Node3D Water { get; set; }
	// [Export] public Node3D WizardTower { get; set; }
	Node3D currentTerrain = null;
	[Export] Node3D spawnPoint;


	public async Task SpawnTerrain(TerrainState.TerrainType terrain, float targetScale)
	{
		if (currentTerrain != null){
			Tween scaleDownTween = GetTree().CreateTween();
			scaleDownTween.TweenMethod(Callable.From((Vector3 scale) => {
				currentTerrain.Scale = scale;
			}), currentTerrain.Scale, Vector3.Zero, totalAnimationDuration).SetEase(Tween.EaseType.Out);
			await ToSignal(scaleDownTween, Tween.SignalName.Finished);
			currentTerrain.QueueFree();

		}
		GD.Print(terrainScenes[terrain]);
		currentTerrain = terrainScenes[terrain].InstantiateUnderAs<Node3D>(spawnPoint);
		currentTerrain.Scale = Vector3.Zero;
		Tween scaleUpTween = GetTree().CreateTween();
		scaleUpTween.TweenMethod(Callable.From((Vector3 scale) => {
			currentTerrain.Scale = scale;
		}), Vector3.Zero, Vector3.One*targetScale, totalAnimationDuration).SetEase(Tween.EaseType.In);
		await ToSignal(GetTree().CreateTimer(totalAnimationDuration - skippedDuration), Timer.SignalName.Timeout);
		// await ToSignal(scaleUpTween, Tween.SignalName.Finished);

	}
}
