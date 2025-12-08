using Godot;

namespace Game;

[GlobalClass]
public partial class PlayerInfo : Resource
{
	[Export] Color troopColor;
	public Color TroopColor => troopColor;
	[Export] Color terrainColor;
	public Color TerrainColor => terrainColor;
	[Export] string name;
	public string Name => name;
}