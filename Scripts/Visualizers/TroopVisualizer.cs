using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Game.State;
using Godot;
using Godot.Collections;

namespace Game.Visualizers;

[GlobalClass]
public partial class TroopVisualizer : Node3D
{
	[Export] Node3D spawnPoint;
	[Export] float appearDuration;
	[Export] float moveDuration;
	[Export] AnimationLibrary troopAnimations;
	[Export] string spawnAnimationName;
	[Export] string idleAnimationName;
	[Export] string movingAnimationName;
	[Export] string deathAnimationName;
	[Export] string damagedAnimationName;
	[Export] string attackAnimationName;
	Node3D spawnedTroop;


	public enum HighlightType
	{
		None,
		Selected,
		Gray,
	}

	[Export] Dictionary<HighlightType, ModelHighlightState> highlightStates;

	public Task Highlight(HighlightType type)
	{
		return highlightStates[type].Transition(spawnedTroop, matManager);
	}

	void SetBaseColor(Color color)
	{
		var mat = new StandardMaterial3D();
		mat.AlbedoColor = color;
		mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		matManager.SetOverlay(mat,ModelMaterialManager.MaterialLevel.Base);
	}
	MeshInstance3D[] troopMeshes;
	AnimationPlayer animationPlayer;
	public async Task Kill()
	{
		if (spawnedTroop == null) return;
		await PlayTroopAnimation(deathAnimationName);
	}

	public async Task Attack(Vector3 target)
	{
		Vector3 delta = target - GlobalPosition;
		LookAt( GlobalPosition - delta,Vector3.Up); // idk why but this works correctly only when reversed
		await PlayTroopAnimation(attackAnimationName, 0.7f);
		DebugDraw3D.DrawArrow(GlobalPosition + Vector3.Up*1f, target + Vector3.Up*1f, Colors.Red,arrow_size:0.1f,  duration:0.3f);
		await ToSignal(GetTree().CreateTimer(0.3f), Timer.SignalName.Timeout);
	}

	public async Task Damaged(int damage)
	{
		await PlayTroopAnimation(damagedAnimationName);
	}

	public async Task MoveTo(Vector3 target)
	{
		Vector3 delta = target - GlobalPosition;
		LookAt( GlobalPosition - delta,Vector3.Up); // idk why but this works correctly only when reversed
		Tween move = GetTree().CreateTween();
		move.TweenMethod(Callable.From((Vector3 newPos) => {
			GlobalPosition = newPos;
		}), GlobalPosition, target, moveDuration).SetEase(Tween.EaseType.Out);
		_ = PlayTroopAnimation(movingAnimationName);
		await ToSignal(move, Tween.SignalName.Finished);
		_ = PlayTroopAnimation(idleAnimationName);
	}

	const string LibraryName = "TroopAnimations";

	async Task PlayTroopAnimation(string animationName, float endSkip = 0)
	{
		var anim = troopAnimations.GetAnimation(animationName);
		animationPlayer.Play($"{LibraryName}/{animationName}");

		await ToSignal(GetTree().CreateTimer(anim.GetLength()-endSkip), Timer.SignalName.Timeout);
	}

	[Export] ModelMaterialManager matManager;
	public async Task Spawn(Vector3 position, TroopData data, Color troopColor)
	{
		GlobalPosition =  position;
		spawnedTroop?.QueueFree();
		spawnedTroop = data.ModelScene.InstantiateUnderAs<Node3D>(spawnPoint);
		matManager.Manage(spawnedTroop);
		animationPlayer = spawnedTroop.GetAllChildrenOfType<AnimationPlayer>().FirstOrDefault();
		Debug.Assert(animationPlayer != null, "Animation player not found under troop scene");


		SetBaseColor(troopColor);
		spawnedTroop.Scale = Vector3.One*0.01f;
		Callable.From(() => {
			spawnedTroop.Scale = Vector3.One;
		}).CallDeferred();
		await PlayTroopAnimation(spawnAnimationName);

		_ = PlayTroopAnimation(idleAnimationName);
		// spawnedTroop.Scale = Vector3.One*0.01f;
		// Tween appear = GetTree().CreateTween();
		// appear.TweenMethod(Callable.From((Vector3 scale) => {
		// 	spawnedTroop.Scale = scale;
		// }), spawnedTroop.Scale, Vector3.One, appearDuration).SetEase(Tween.EaseType.Out);
		// await ToSignal(appear, Tween.SignalName.Finished);
	}
}