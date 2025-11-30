using Game.State;
using Godot;

namespace Game;

public class HexCell(Vector2I hexCoords, TerrainState.TerrainType terrain)
{
	
	public Vector2I HexCoords { get; } = hexCoords;

	public TerrainState.TerrainType Terrain { get; set; } = terrain;

	//Todo: move this to a world state
	public bool IsOccupied { get; set; } = false;
}
