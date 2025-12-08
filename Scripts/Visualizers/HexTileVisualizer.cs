using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Game.State;
using Godot;
using Godot.Collections;

namespace Game.Visualizers;

public partial class HexTileVisualizer : Node3D
{


	public enum HighlightType
	{
		None,
		Selected,
		Gray,
	}
	[Export] Dictionary<HighlightType, ModelHighlightState> highlightStates;
	public Task Highlight(HighlightType type)
	{
		return highlightStates[type].Transition(currentTerrain, matManager);
	}

	public async Task SetBaseColor(Color color)
	{
		var mat = new StandardMaterial3D();
		mat.AlbedoColor = color;
		mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		matManager.SetOverlay(mat,ModelMaterialManager.MaterialLevel.Base);
	}
	[Export] Dictionary<TerrainState.TerrainType, PackedScene> terrainScenes;

	[Export] float totalAnimationDuration = 0.5f;

	[Export] float skippedDuration = 0.2f;
	Node3D currentTerrain;
	[Export] Node3D spawnPoint;
	[Export] ModelMaterialManager matManager;
	public Vector2I HexCoord { get; set; } // Added property for the hex coordinate


	public async Task SpawnTerrain(TerrainState.TerrainType terrain, float targetScale)
	{
		if (currentTerrain != null){
			Tween scaleDownTween = GetTree().CreateTween();
			scaleDownTween.TweenMethod(Callable.From((Vector3 scale) => {
				Scale = scale;
			}), Scale, Vector3.One * 0.01f, totalAnimationDuration).SetEase(Tween.EaseType.Out);
			await ToSignal(scaleDownTween, Tween.SignalName.Finished);
			currentTerrain.QueueFree();

		}
		currentTerrain = terrainScenes[terrain].InstantiateUnderAs<Node3D>(spawnPoint);
		matManager.Manage(currentTerrain);
		currentTerrain.Scale = Vector3.One;
		Tween scaleUpTween = GetTree().CreateTween();
		scaleUpTween.TweenMethod(Callable.From((Vector3 scale) => {
			Scale = scale;
		}), Vector3.One * 0.01f, Vector3.One*targetScale, totalAnimationDuration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Bounce);
		await ToSignal(GetTree().CreateTimer(totalAnimationDuration - skippedDuration), Timer.SignalName.Timeout);
		// await ToSignal(scaleUpTween, Tween.SignalName.Finished);

	}
}
