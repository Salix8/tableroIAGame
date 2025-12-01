using Godot;
using Game.State;
using System.Linq;

namespace Game.AI;

[GlobalClass]
public partial class InfluenceMapManager : Node
{
	public InfluenceMap InterestMap { get; private set; } = new();
	public InfluenceMap SecurityMap { get; private set; } = new();

	[Export] private float _decayFactor = 0.5f;
	[Export] private int _securityPropagation = 3;

	public void UpdateMaps(WorldState state, int aiPlayerIndex)
	{
		// 1. Mapa de Interés (Bosques = 1, Agua = 0.5, Mana = 1)
		InterestMap.Clear();
		foreach (var pos in state.TerrainState.GetFilledPositions())
		{
			var type = state.TerrainState.GetTerrainType(pos);
			float score = type switch
			{
				TerrainState.TerrainType.Forest => 1.0f,
				TerrainState.TerrainType.ManaPool => 1.0f,
				TerrainState.TerrainType.Water => 0.5f, // Terreno dificil, pero no nulo
				_ => 0.1f
			};
			InterestMap.AddInfluence(pos, score);
		}
		// No propagamos interés (o muy poco) para que la IA vaya a la casilla exacta

		// 2. Mapa de Seguridad (Enemigos = Peligro)
		SecurityMap.Clear();
		foreach (var player in state.PlayerStates)
		{
			if (player.PlayerIndex == aiPlayerIndex) continue; // Mis tropas no son peligro

			foreach (var troop in player.Troops)
			{
				// Usamos daño como amenaza, si no tienes daño accesible usa 10f
				float threat = troop.Data.Damage > 0 ? troop.Data.Damage : 10f;
				SecurityMap.AddInfluence(troop.Position, threat);
			}
		}
		SecurityMap.Propagate(_decayFactor, _securityPropagation);
	}

	public float GetTileScore(Vector2I coords)
	{
		return InterestMap.GetInfluence(coords) - SecurityMap.GetInfluence(coords);
	}
}
