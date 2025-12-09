using Godot;
using Game.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.AI;

[GlobalClass]
public partial class InfluenceMapManager : Node
{
    public InfluenceMap TerritoryMap { get; private set; } = new();
    public InfluenceMap ThreatMap { get; private set; } = new();

    public void UpdateMaps(WorldState state, PlayerId aiPlayerId)
    {
        var troopMap = TroopMap(aiPlayerId, state);
        var manaMap = ManaClaimMap(aiPlayerId, state);
        var combined = troopMap.Clone();
        combined.Combine(manaMap, (troopVal, manaVal) => troopVal*1f + manaVal*0.5f);
        combined.ApplyConvolution(state.TerrainState.GetFilledPositions(), 0.5f, 0.5f/6, 15);
        combined.Normalize(-1,1);
        TerritoryMap = combined;
        // 3. APLICAR CONVOLUCIÓN (Blur/Propagate)
        // Esto expande el peligro de "la casilla donde está el enemigo" a "las casillas adyacentes"
        // ThreatMap.ApplyConvolution(allMapCoords, _decayCenter, _decayNeighbor, _blurIterations);
        //
        // // El interés también se puede difuminar un poco para que la IA se acerque
        // InterestMap.ApplyConvolution(allMapCoords, 0.5f, 0.08f, 1);
    }

    InfluenceMap TroopMap(PlayerId player, WorldState state)
    {
        var map = new InfluenceMap();
        var troops = state.GetTroops();
        map.GenerateBaseMap(troops.Keys, (pos => troops[pos].Owner == player ? 1 : -1));
        return map;
    }

    InfluenceMap ManaClaimMap(PlayerId playerId, WorldState state)
    {
        var map = new InfluenceMap();
        map.GenerateBaseMap(state.PlayerManaClaims.Keys, pos => state.PlayerManaClaims[pos] == playerId ? 1 : -1);
        return map;
    }


    public float GetFinalScore(Vector2I coords)
    {
        return TerritoryMap.GetInfluence(coords) ;
    }
}