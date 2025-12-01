using Game.State;
using Godot;

namespace Game.Visualizers;

[GlobalClass]
public partial class PlayerInputHandler : Node
{
    [Export]
    private HexGrid3D _grid; // To convert world coordinates to hex coordinates

            [Export]

            private PlayableWorldState _playableWorldState; // To pass the clicked hex to the game state manager

    

            [Export(PropertyHint.Layers3DPhysics)]

            private uint _collisionMask = 2; // Layer 2, as instructed to the user

    

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

    

                    if (result.TryGetValue("position", out Variant hitPositionVariant))

                    {

                        Vector3 hitPosition = hitPositionVariant.AsVector3();

                        Vector2I hexCoord = _grid.WorldToHex(hitPosition);

    

                        GD.Print($"Player clicked hex: {hexCoord}");

                        _playableWorldState.HandlePlayerClick(hexCoord); // Now calls PlayableWorldState

                    }

                }

            }

    
}
