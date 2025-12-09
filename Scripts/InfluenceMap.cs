using System;
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Godot;

namespace Game;

public class InfluenceMap()
{
    Dictionary<Vector2I, float> map = new();

    public InfluenceMap Clone()
    {
        return new InfluenceMap{
            map = map.ToDictionary()
        };
    }
    public void Clear() => map.Clear();
    public float GetInfluence(Vector2I coords) => map.GetValueOrDefault(coords, 0f);
    public void AddInfluence(Vector2I coords, float value)
    {
        map.TryAdd(coords, 0f);
        map[coords] += value;
    }
    public void GenerateBaseMap(IEnumerable<Vector2I> coords, Func<Vector2I, float> valueCalc)
    {
        map.Clear();
        foreach (var pos in coords)
        {
            float val = valueCalc(pos);
            map[pos] = val;
        }
    }

    public void ApplyConvolution(IEnumerable<Vector2I> convolvedArea, float centerWeight, float neighborWeight, int iterations = 1)
    {
        Vector2I[] area = convolvedArea.ToArray();
        for (int i = 0; i < iterations; i++)
        {
            var nextMap = new Dictionary<Vector2I, float>();
            foreach (var pos in area)
            {
                float newInfluence = 0f;

                newInfluence += GetInfluence(pos) * centerWeight;

                foreach (var neighbor in HexGrid.GetNeighborCoords(pos))
                {
                    newInfluence += GetInfluence(neighbor) * neighborWeight;
                }
                nextMap[pos] = newInfluence;
            }
            map = nextMap;
        }
    }

    public void Combine(InfluenceMap other, Func<float, float,float> operation)
    {
        var joinedKeys = map.Keys.Union(other.map.Keys).ToHashSet();
        foreach (Vector2I key in joinedKeys){
            map[key] = operation(GetInfluence(key), other.GetInfluence(key));
        }
    }

    public IReadOnlyDictionary<Vector2I, float> Map => map;

    public void Normalize(int newMin = 0, int newMax = 1)
    {
        if (map.Count == 0) return;

        float max = float.MinValue;
        float min = float.MaxValue;

        foreach(var val in map.Values)
        {
            if(val > max) max = val;
            if(val < min) min = val;
        }

        if (Mathf.IsEqualApprox(max, min)) return;

        var keys = new List<Vector2I>(map.Keys);
        foreach(var key in keys)
        {
            map[key] = Mathf.Remap(map[key], min, max, newMin, newMax);
        }
    }
}