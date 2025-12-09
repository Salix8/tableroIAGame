using Godot;
using Game.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.AI;

[GlobalClass]
public partial class InfluenceMapManager : Node
{
    public InfluenceMap InterestMap { get; private set; } = new();
    public InfluenceMap ThreatMap { get; private set; } = new();

    // Configuración de Convolución
    [Export] private float _decayCenter = 0.4f; // Cuánto se queda en la casilla original
    [Export] private float _decayNeighbor = 0.1f; // Cuánto se va a cada vecino (0.1 * 6 = 0.6 + 0.4 = 1.0 -> Energía conservada)
    [Export] private int _blurIterations = 2;

    public void UpdateMaps(WorldState state, PlayerId aiPlayerId, IEnumerable<Vector2I> allMapCoords)
    {
        // 1. DEFINIR LAS FUNCIONES DE EVALUACIÓN (KERNEL LOGICO)

        // Función A: ¿Qué tan interesante es esta casilla?
        Func<Vector2I, WorldState, float> interestEvaluator = (pos, s) =>
        {
            float score = 0f;
            
            // Ejemplo: Terreno
            var type = s.TerrainState.GetTerrainType(pos);
            if (type.HasValue)
            {
                if (type.Value == TerrainState.TerrainType.Forest) score += 1.5f;
                if (type.Value == TerrainState.TerrainType.ManaPool) score += 5.0f; // Muy valioso
            }

            // Ejemplo: Unidades enemigas débiles son interesantes para atacar
            if (s.TryGetTroop(pos, out var troop))
            {
                if (troop.Owner != aiPlayerId && troop.CurrentHealth < 10)
                {
                    score += 3.0f;
                }
            }

            return score;
        };

        // Función B: ¿Qué tan peligrosa es esta casilla?
        Func<Vector2I, WorldState, float> threatEvaluator = (pos, s) =>
        {
            float score = 0f;
            // Buscamos unidades enemigas fuertes
            if (s.TryGetTroop(pos, out var troop))
            {
                if (troop.Owner != aiPlayerId)
                {
                    // El peligro es su daño potencial
                    score += troop.Data.Damage;
                }
            }
            return score;
        };

        // 2. GENERAR MAPAS BASE (Seed)
        InterestMap.GenerateBaseMap(state, allMapCoords, interestEvaluator);
        ThreatMap.GenerateBaseMap(state, allMapCoords, threatEvaluator);

        // 3. APLICAR CONVOLUCIÓN (Blur/Propagate)
        // Esto expande el peligro de "la casilla donde está el enemigo" a "las casillas adyacentes"
        ThreatMap.ApplyConvolution(allMapCoords, _decayCenter, _decayNeighbor, _blurIterations);

        // El interés también se puede difuminar un poco para que la IA se acerque
        InterestMap.ApplyConvolution(allMapCoords, 0.5f, 0.08f, 1);
    }

    public float GetFinalScore(Vector2I coords)
    {
        // Fórmula básica: Interés - Peligro
        return InterestMap.GetInfluence(coords) - ThreatMap.GetInfluence(coords);
    }
}