using System.Threading.Tasks;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class ModelHighlightState : Resource
{
	[Export] float scale = 1;
	[Export] float elevation;
	[Export] Material overrideMaterial;
	[Export] Material overlayMaterial;
	[Export] float duration = 0.3f;
	[Export] Tween.EaseType easeType = Tween.EaseType.InOut;
	[Export] Tween.TransitionType transitionType = Tween.TransitionType.Quad;
	public async Task Transition(Node3D modelParent, ModelMaterialManager materialManager)
	{
		materialManager.SetOverlay(overlayMaterial, ModelMaterialManager.MaterialLevel.Extra);
		materialManager.SetOverride(overrideMaterial);

		SceneTree tree = GodotExtensions.TryGetSceneTree();
		if (tree == null) return;

		Tween transition = tree.CreateTween();
		transition.SetParallel();
		transition.TweenMethod(Callable.From((Vector3 newPos) => {
			modelParent.Position = newPos;
		}), modelParent.Position, modelParent.Position with{Y = elevation}, duration).SetEase(easeType).SetTrans(transitionType);
		transition.TweenMethod(Callable.From((Vector3 newScale) => {
			modelParent.Scale = newScale;
		}), modelParent.Scale, new Vector3(scale,scale,scale),  duration).SetEase(easeType).SetTrans(transitionType);

		await ToSignal(transition, Tween.SignalName.Finished);
	}
}