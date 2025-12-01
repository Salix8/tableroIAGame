using System.Threading.Tasks;
using Godot;
using Game;

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

	async Task MoveTo(Vector2I pos)
	{
		if (grid == null) return;
		Vector3 targetPosition = grid.HexToWorld(pos);
		LookAt(targetPosition,Vector3.Up);
		Tween move = GetTree().CreateTween();
		move.TweenMethod(Callable.From((Vector3 newPos) => {
			GlobalPosition = newPos;
		}), GlobalPosition, targetPosition, moveDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(move, Tween.SignalName.Finished);
	}

	public async Task Spawn(Troop troop, HexGrid3D troopGrid)
	{
		grid = troopGrid;
		troop.MovedTo.Subscribe(MoveTo);
		troop.Killed.Subscribe(Kill);
		GlobalPosition = grid.HexToWorld(troop.Position);
		spawnedTroop?.QueueFree();
		
		spawnedTroop = troop.Data.ModelScene.InstantiateUnderAs<Node3D>(spawnPoint);

		// Find the Area3D within the spawned model to make it clickable
		var area = spawnedTroop.FindChild("TroopClickArea", recursive: true) as Area3D;
		if (area != null)
		{
			// Layer 3 for "Interactables" (1 << 2 means the 3rd bit is 1)
			area.CollisionLayer = 1 << 2; 
			// Store the logical troop's ID (as a string) in the area's metadata
			area.SetMeta("troop_id", troop.Id.ToString());
		}
		else
		{
			GD.PrintErr($"Troop model '{spawnedTroop.Name}' is missing an Area3D named 'TroopClickArea'. It will not be clickable.");
		}

		spawnedTroop.Scale = Vector3.Zero;
		Tween appear = GetTree().CreateTween();
		appear.TweenMethod(Callable.From((Vector3 scale) => {
			spawnedTroop.Scale = scale;
		}), spawnedTroop.Scale, Vector3.One, appearDuration).SetEase(Tween.EaseType.Out);

		await ToSignal(appear, Tween.SignalName.Finished);
	}
}
