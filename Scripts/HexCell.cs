using Godot;

namespace Game;

public class HexCell(Vector2I hexCoords, HexCell.TerrainType terrain)
{
	public enum TerrainType {
		Plains,
		Forest,
		Mountain,
		Water
	}
	
	public Vector2I HexCoords { get; } = hexCoords;

	public TerrainType Terrain { get; set; } = terrain;

	//Todo: move this to a world state
	public bool IsOccupied { get; set; } = false;
}
