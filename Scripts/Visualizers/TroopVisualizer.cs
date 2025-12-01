using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class TroopVisualizer : Node3D
{
	[Export] Node3D spawnPoint;
	[Export] float appearDuration;
	[Export] float moveDuration;
	Node3D spawnedTroop;
	HexGrid3D grid;

	async Task Kill()
	{
		if (grid == null) return;
		if (spawnedTroop == null) return;

		Tween disappear = GetTree().CreateTween();
		disappear.TweenMethod(Callable.From((Vector3 scale) => {
			spawnedTroop.Scale = scale;
		}), spawnedTroop.Scale, Vector3.Zero, appearDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(disappear, Tween.SignalName.Finished);
		QueueFree();
	}

	async Task Attack(Vector2I coord)
	{
		Vector3 target = grid.HexToWorld(coord);
		LookAt(target,Vector3.Up);
		DebugDraw3D.DrawArrow(GlobalPosition + Vector3.Up*1f, target + Vector3.Up*1f, Colors.Red,arrow_size:0.1f,  duration:0.3f);
		await ToSignal(GetTree().CreateTimer(0.3f), Timer.SignalName.Timeout);
	}

	async Task Damaged(int damage)
	{

		await ToSignal(GetTree().CreateTimer(0.3f), Timer.SignalName.Timeout);
	}

	async Task MoveTo(Vector2I coord)
	{
		if (grid == null) return;
		Vector3 targetPosition = grid.HexToWorld(coord);
		LookAt(targetPosition,Vector3.Up);
		Tween move = GetTree().CreateTween();
		move.TweenMethod(Callable.From((Vector3 newPos) => {
			GlobalPosition = newPos;
		}), GlobalPosition, targetPosition, moveDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(move, Tween.SignalName.Finished);
	}

	public void Initialize(HexGrid3D hexGrid)
	{
		grid = hexGrid;
	}

	public async Task Spawn(TroopManager.Troop troop, ITroopEventsHandler troopEventsHandler)
	{
		troopEventsHandler.GetTroopMovedHandler().Subscribe(MoveTo);
		troopEventsHandler.GetTroopKilledHandler().Subscribe(Kill);
		troopEventsHandler.GetTroopAttackingHandler().Subscribe(Attack);
		troopEventsHandler.GetTroopDamagedHandler().Subscribe(Damaged);
		GlobalPosition = grid.HexToWorld(troop.Position);
		spawnedTroop?.QueueFree();
		spawnedTroop = troop.Data.ModelScene.InstantiateUnderAs<Node3D>(spawnPoint);
		spawnedTroop.Scale = Vector3.Zero;
		Tween appear = GetTree().CreateTween();
		appear.TweenMethod(Callable.From((Vector3 scale) => {
			spawnedTroop.Scale = scale;
		}), spawnedTroop.Scale, Vector3.One, appearDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(appear, Tween.SignalName.Finished);
	}
}