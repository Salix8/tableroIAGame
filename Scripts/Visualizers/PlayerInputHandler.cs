using Game.State;
using Godot;
using System; // Added for Guid

namespace Game.Visualizers;

[GlobalClass]
public partial class PlayerInputHandler : Node
{
    [Export]
    private HexGrid3D _grid;

    [Export]
    private PlayableWorldState _playableWorldState;

    [Export(PropertyHint.Layers3DPhysics)]
    private uint _collisionMask = 6; // Default to layers 2 (Ground) and 3 (Interactables)

    private Camera3D _camera;

    public override void _Ready()
    {
        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
        {
            GD.PrintErr("PlayerInputHandler: No Camera3D found in viewport!");
            SetProcessUnhandledInput(false);
        }

        if (_grid == null)
        {
            GD.PrintErr("PlayerInputHandler: HexGrid3D not assigned!");
            SetProcessUnhandledInput(false);
        }

        if (_playableWorldState == null)
        {
            GD.PrintErr("PlayerInputHandler: PlayableWorldState not assigned!");
            SetProcessUnhandledInput(false);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_camera == null || _grid == null || _playableWorldState == null)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.IsPressed() && mouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector2 mousePosition = mouseButton.Position;

            Vector3 rayOrigin = _camera.ProjectRayOrigin(mousePosition);
            Vector3 rayNormal = _camera.ProjectRayNormal(mousePosition);
            Vector3 rayEnd = rayOrigin + rayNormal * 1000.0f;

            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
            query.CollideWithAreas = true;
            query.CollisionMask = _collisionMask;

            Godot.Collections.Dictionary result = _camera.GetWorld3D().DirectSpaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node>();

                // Check if we hit a troop by checking for metadata
                if (collider.HasMeta("troop_id"))
                {
                    var troopIdString = collider.GetMeta("troop_id").As<string>();
                    if (Guid.TryParse(troopIdString, out Guid troopId))
                    {
                        Troop troop = _playableWorldState.GetTroopById(troopId);
                        if (troop != null)
                        {
                            _playableWorldState.HandleClickOnTroop(troop);
                            return; // Prioritize the troop click and stop further processing
                        }
                    }
                    GD.PrintErr($"PlayerInputHandler: Could not parse troop ID from metadata: {troopIdString}");
                }
                // Check if we hit a mana pool by checking for metadata
                else if (collider.HasMeta("mana_pool_coord"))
                {
                    Vector2I coord = collider.GetMeta("mana_pool_coord").As<Vector2I>();
                    _playableWorldState.HandleClickOnManaPool(coord);
                    return; // Prioritize the mana pool click
                }
                
                // If no troop or mana pool was hit, it's a click on the ground plane. Get hex coordinate.
                Vector3 hitPosition = result["position"].AsVector3();
                Vector2I hexCoord = _grid.WorldToHex(hitPosition);
                _playableWorldState.HandleClickOnHex(hexCoord);
            }
        }
    }
}
