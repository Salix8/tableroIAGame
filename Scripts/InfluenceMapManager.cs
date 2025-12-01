using Godot;
using Game.State;
using System.Linq;

namespace Game.AI;

[GlobalClass]
public partial class InfluenceMapManager : Node
{
	public InfluenceMap InterestMap { get; private set; } = new();
	public InfluenceMap SecurityMap { get; private set; } = new(); // Este será el mapa de Amenaza/Proximidad

	[Export] private float _decayFactor = 0.5f; // Cuánto se difumina el peligro por casilla
	[Export] private int _securityPropagation = 4; // Radio de visión de amenaza (4 casillas)

	public void UpdateMaps(WorldState state, int aiPlayerIndex)
	{
		InterestMap.Clear();
		foreach (var pos in state.TerrainState.GetFilledPositions())
		{
			var type = state.TerrainState.GetTerrainType(pos);
			float score = type switch
			{
				TerrainState.TerrainType.Forest => 1.5f,
				TerrainState.TerrainType.ManaPool => 2.0f,
				TerrainState.TerrainType.Water => 0.5f,
				_ => 0.1f
			};
			InterestMap.AddInfluence(pos, score);
		}
		SecurityMap.Clear();
		
		foreach (var player in state.PlayerStates)
		{
			if (player.PlayerIndex == aiPlayerIndex) continue;

			foreach (var troop in player.Troops)
			{
				float threatValue = troop.Data.Damage > 0 ? troop.Data.Damage : 5f; 
				
				SecurityMap.AddInfluence(troop.Position, threatValue);
			}
		}

		SecurityMap.Propagate(_decayFactor, _securityPropagation);
	}

	public float GetTileScore(Vector2I coords)
	{
		return InterestMap.GetInfluence(coords) - SecurityMap.GetInfluence(coords);
	}
}
