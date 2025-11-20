using Godot;
using Game.State;

namespace Game;

public partial class HexTile : Node3D
{
    [Export] public Node3D Plains { get; set; }
    [Export] public Node3D Forest { get; set; }
    [Export] public Node3D Mountain { get; set; }
    [Export] public Node3D Water { get; set; }

    public void SetTerrain(TerrainState.TerrainType terrain)
    {
        Plains.Visible = terrain == TerrainState.TerrainType.Plains;
        Forest.Visible = terrain == TerrainState.TerrainType.Forest;
        Mountain.Visible = terrain == TerrainState.TerrainType.Mountain;
        Water.Visible = terrain == TerrainState.TerrainType.Water;
    }
}
