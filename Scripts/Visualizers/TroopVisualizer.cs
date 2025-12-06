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
		return highlightStates[type].Transition(spawnedTroop, troopMeshes);
	}

	public void SetBaseColor(Color color)
	{
		foreach (MeshInstance3D troopMesh in troopMeshes){
			int count = troopMesh.Mesh.GetSurfaceCount();
			for (int i = 0; i < count; i++){
				if (troopMesh.Mesh.SurfaceGetMaterial(i) is StandardMaterial3D mat) mat.AlbedoColor = color;
			}
		}
	}
	MeshInstance3D[] troopMeshes;

	public async Task Kill()
	{
		if (spawnedTroop == null) return;

		Tween disappear = GetTree().CreateTween();
		disappear.TweenMethod(Callable.From((Vector3 scale) => {
			spawnedTroop.Scale = scale;
		}), spawnedTroop.Scale, Vector3.Zero, appearDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(disappear, Tween.SignalName.Finished);
	}

	public async Task Attack(Vector3 target)
	{
		LookAt(target,Vector3.Up);
		DebugDraw3D.DrawArrow(GlobalPosition + Vector3.Up*1f, target + Vector3.Up*1f, Colors.Red,arrow_size:0.1f,  duration:0.3f);
		await ToSignal(GetTree().CreateTimer(0.3f), Timer.SignalName.Timeout);
	}

	public async Task Damaged(int damage)
	{

		await ToSignal(GetTree().CreateTimer(0.3f), Timer.SignalName.Timeout);
	}

	public async Task MoveTo(Vector3 target)
	{
		LookAt(target,Vector3.Up);
		Tween move = GetTree().CreateTween();
		move.TweenMethod(Callable.From((Vector3 newPos) => {
			GlobalPosition = newPos;
		}), GlobalPosition, target, moveDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(move, Tween.SignalName.Finished);
	}

	public async Task Spawn(Vector3 position, TroopData data)
	{
		GlobalPosition =  position;
		spawnedTroop?.QueueFree();
		spawnedTroop = data.ModelScene.InstantiateUnderAs<Node3D>(spawnPoint);

		troopMeshes = spawnedTroop.GetAllChildrenOfType<MeshInstance3D>().ToArray();
		Debug.Assert(spawnedTroop != null, "No troop mesh found under troop scene");
		spawnedTroop.Scale = Vector3.Zero;
		Tween appear = GetTree().CreateTween();
		appear.TweenMethod(Callable.From((Vector3 scale) => {
			spawnedTroop.Scale = scale;
		}), spawnedTroop.Scale, Vector3.One, appearDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(appear, Tween.SignalName.Finished);
	}
}