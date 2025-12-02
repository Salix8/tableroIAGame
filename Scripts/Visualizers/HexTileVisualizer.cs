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
	public Vector2I HexCoord { get; set; } // Added property for the hex coordinate


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

		// If it's a ManaPool, make it clickable
		if (terrain == TerrainState.TerrainType.ManaPool || terrain == TerrainState.TerrainType.WizardTower) // Using WizardTower as a common special tile, adjust if ManaPool has its own distinct scene
		{
			var area = currentTerrain.FindChild("ManaPoolClickArea", recursive: true) as Area3D;
			if (area != null)
			{
				area.CollisionLayer = 1 << 2; // Layer 3 for "Interactables"
				area.SetMeta("mana_pool_coord", HexCoord); // Store the hex coordinate
			}
			else
			{
				GD.PrintErr($"ManaPool/WizardTower model '{currentTerrain.Name}' is missing an Area3D named 'ManaPoolClickArea'. It will not be clickable.");
			}
		}

		currentTerrain.Scale = Vector3.Zero;
		Tween scaleUpTween = GetTree().CreateTween();
		scaleUpTween.TweenMethod(Callable.From((Vector3 scale) => {
			currentTerrain.Scale = scale;
		}), Vector3.Zero, Vector3.One*targetScale, totalAnimationDuration).SetEase(Tween.EaseType.In);
		await ToSignal(GetTree().CreateTimer(totalAnimationDuration - skippedDuration), Timer.SignalName.Timeout);
		// await ToSignal(scaleUpTween, Tween.SignalName.Finished);

	}
}
