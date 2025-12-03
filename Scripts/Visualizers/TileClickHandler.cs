using Game.State;
using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot.Collections;

namespace Game.Visualizers;

[GlobalClass]
public partial class TileClickHandler : Node
{
	[Export] HexGrid3D grid;

	[Export] PlayableWorldState playableWorldState;

	[Export(PropertyHint.Layers3DPhysics)] uint collisionMask = 6; // Default to layers 2 (Ground) and 3 (Interactables)
	[Export(PropertyHint.Layers3DPhysics)] uint troopLayers = 2;
	[Export(PropertyHint.Layers3DPhysics)] uint groundLayers = 3;
	[Export] Camera3D camera;
	public event Action<Vector2I> TileClicked;

	public Task<Vector2I> WaitForTileClick()
	{
		TaskCompletionSource<Vector2I> source = new();
		TileClicked += OnTileClicked;
		return source.Task;

		void OnTileClicked(Vector2I tile)
		{
			source.SetResult(tile);
			TileClicked -= OnTileClicked;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		Debug.Assert(grid != null, "grid is null!");
		Debug.Assert(playableWorldState != null, "playableWorldState is null!");
		Debug.Assert(camera != null, "camera is null!");
		if (@event is not InputEventMouseButton mouseButton || !mouseButton.IsPressed() ||
		    mouseButton.ButtonIndex != MouseButton.Left) return;
		Vector2 mousePosition = mouseButton.Position;

		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayNormal = camera.ProjectRayNormal(mousePosition);
		Vector3 rayEnd = rayOrigin + rayNormal * 1000.0f;

		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		query.CollideWithAreas = true;
		query.CollisionMask = collisionMask;

		Dictionary result = camera.GetWorld3D().DirectSpaceState.IntersectRay(query);

		if (result.Count <= 0) return;

		Vector3 rayPos = result["position"].AsVector3();
		Vector2I tile = grid.WorldToHex(rayPos);
		// DebugDraw3D.DrawSphere(tileWorld, 0.5f, Colors.Chartreuse, duration: 1);
		// DebugDraw3D.DrawSphere(rayPos, 0.5f, Colors.DarkGreen, duration: 1);
		TileClicked?.Invoke(tile);


		// // Check if we hit a troop by checking for metadata
		// if (collider.HasMeta("troop_id"))
		// {
		//     var troopIdString = collider.GetMeta("troop_id").As<string>();
		//     if (Guid.TryParse(troopIdString, out Guid troopId))
		//     {
		//         TroopManager.Troop troop = playableWorldState.GetTroopById(troopId);
		//         if (troop != null)
		//         {
		//             playableWorldState.HandleClickOnTroop(troop);
		//             return; // Prioritize the troop click and stop further processing
		//         }
		//     }
		//     GD.PrintErr($"PlayerInputHandler: Could not parse troop ID from metadata: {troopIdString}");
		// }
		// // Check if we hit a mana pool by checking for metadata
		// else if (collider.HasMeta("mana_pool_coord"))
		// {
		//     Vector2I coord = collider.GetMeta("mana_pool_coord").As<Vector2I>();
		//     playableWorldState.HandleClickOnManaPool(coord);
		//     return; // Prioritize the mana pool click
		// }
		//
		// // If no troop or mana pool was hit, it's a click on the ground plane. Get hex coordinate.
		// Vector3 hitPosition = result["position"].AsVector3();
		// Vector2I hexCoord = grid.WorldToHex(hitPosition);
		// playableWorldState.HandleClickOnHex(hexCoord);
	}
}