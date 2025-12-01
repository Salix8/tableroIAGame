using Godot;
using System.Collections.Generic;
using Game;

namespace Game.AI;

public class InfluenceMap
{
	private Dictionary<Vector2I, float> _map = new();

	public void Clear() => _map.Clear();

	public void AddInfluence(Vector2I coords, float value)
	{
		if (!_map.ContainsKey(coords)) _map[coords] = 0f;
		_map[coords] += value;
	}

	public float GetInfluence(Vector2I coords) => _map.GetValueOrDefault(coords, 0f);

	public void Propagate(float decay, int iterations)
	{
		for (int i = 0; i < iterations; i++)
		{
			var nextMap = new Dictionary<Vector2I, float>(_map);
			foreach (var kvp in _map)
			{
				if (kvp.Value <= 0.01f) continue;
				foreach (var neighbor in HexGrid3D.GetNeighborCoords(kvp.Key))
				{
					if (!nextMap.ContainsKey(neighbor)) nextMap[neighbor] = 0f;
					nextMap[neighbor] += kvp.Value * decay;
				}
			}
			_map = nextMap;
		}
	}
}
