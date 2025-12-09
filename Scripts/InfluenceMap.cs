using Godot;
using System;
using System.Collections.Generic;
using Game.State;

namespace Game.AI;

public class InfluenceMap
{
    // Usamos un Diccionario para flexibilidad, pero si el mapa es fijo, un array 2D/1D sería más rápido.
    private Dictionary<Vector2I, float> _map = new();

    // Limpia el mapa
    public void Clear() => _map.Clear();

    // Obtiene valor con seguridad (devuelve 0 si no existe)
    public float GetInfluence(Vector2I coords) => _map.GetValueOrDefault(coords, 0f);

    // Añade valor manual (útil para casos puntuales)
    public void AddInfluence(Vector2I coords, float value)
    {
        if (!_map.ContainsKey(coords)) _map[coords] = 0f;
        _map[coords] += value;
    }

    /// <summary>
    /// Paso 1: Generación Base (Seeding)
    /// Recorre una lista de coordenadas dadas y calcula su valor inicial usando la función proporcionada.
    /// Esto es lo que pedías: construye el diccionario basándose en el WorldState.
    /// </summary>
    /// <param name="state">El estado actual del mundo.</param>
    /// <param name="gridCoords">Todas las coordenadas válidas del mapa.</param>
    /// <param name="valueCalc">Tu función kernel: recibe pos y estado, devuelve float.</param>
    public void GenerateBaseMap(WorldState state, IEnumerable<Vector2I> gridCoords, Func<Vector2I, WorldState, float> valueCalc)
    {
        _map.Clear();
        foreach (var pos in gridCoords)
        {
            float val = valueCalc(pos, state);
            // Solo guardamos si es relevante para ahorrar memoria/procesamiento
            if (Mathf.Abs(val) > 0.001f)
            {
                _map[pos] = val;
            }
        }
    }

    /// <summary>
    /// Paso 2: Filtro de Convolución Hexagonal
    /// Aplica un suavizado o difusión usando pesos para la celda central y sus 6 vecinos.
    /// </summary>
    /// <param name="centerWeight">Peso de la celda actual (ej. 0.4).</param>
    /// <param name="neighborWeight">Peso de los vecinos (ej. 0.1).</param>
    /// <param name="iterations">Cuántas veces pasar el filtro.</param>
    public void ApplyConvolution(IEnumerable<Vector2I> allMapCoords, float centerWeight, float neighborWeight, int iterations = 1)
    {
        for (int i = 0; i < iterations; i++)
        {
            var nextMap = new Dictionary<Vector2I, float>();
            foreach (var pos in allMapCoords)
            {
                float newInfluence = 0f;

                // Contribución de la propia celda
                newInfluence += GetInfluence(pos) * centerWeight;
                
                // Contribución de los vecinos
                foreach (var neighbor in HexGrid.GetNeighborCoords(pos))
                {
                    newInfluence += GetInfluence(neighbor) * neighborWeight;
                }

                if (Mathf.Abs(newInfluence) > 0.001f)
                {
                    nextMap[pos] = newInfluence;
                }
            }
            _map = nextMap;
        }
    }

    // Helper para sumar valores al nuevo mapa
    private void AddValueToMap(Dictionary<Vector2I, float> map, Vector2I pos, float value)
    {
        if (!map.ContainsKey(pos)) map[pos] = 0f;
        map[pos] += value;
    }

    public IReadOnlyDictionary<Vector2I, float> GetAllInfluences() => _map;

    // Función de normalización (opcional): Hace que el valor más alto sea 1 y el más bajo 0
    public void Normalize()
    {
        if (_map.Count == 0) return;

        float max = float.MinValue;
        float min = float.MaxValue;

        foreach(var val in _map.Values)
        {
            if(val > max) max = val;
            if(val < min) min = val;
        }

        if (Mathf.IsEqualApprox(max, min)) return;

        var keys = new List<Vector2I>(_map.Keys);
        foreach(var key in keys)
        {
            _map[key] = (_map[key] - min) / (max - min);
        }
    }
}