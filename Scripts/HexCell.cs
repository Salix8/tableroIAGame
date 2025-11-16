using Godot;

namespace Game;

public class HexCell
{
    public enum TerrainType {
        Plains,
        Forest,
        Mountain,
        Water
    }
    
    public Vector2I Coords { get; }
    public TerrainType Terrain { get; set; }
    public bool IsOccupied { get; set; } = false;

    public HexCell(Vector2I coords, TerrainType terrain)
    {
        Coords = coords;
        Terrain = terrain;
    }
}
