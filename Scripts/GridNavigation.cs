using Godot;
using Game;
using System.Collections.Generic;
using System.Linq;
using Game.State;

public partial class GridNavigation : Node
{

	// public HashSet<HexCell> ComputeReachableArea(Vector2I startCoords, int maxMovementCost)
	// {
	// 	var reachable = new HashSet<Vector2I>();
	// 	var openSet = new PriorityQueue<Vector2I, int>();
	// 	var gCost = new Dictionary<Vector2I, int>();
	//
	// 	HexCell startCell = startCoords;
	// 	if (startCell == null) return reachable;
	//
	// 	openSet.Enqueue(startCell, 0);
	// 	gCost[startCell] = 0;
	// 	reachable.Add(startCell);
	//
	// 	while (openSet.TryDequeue(out HexCell current, out int currentCost))
	// 	{
	// 		if (currentCost > maxMovementCost) continue;
	//
	// 		foreach (var neighbor in hexGrid.GetNeighbors(current.HexCoords))
	// 		{
	// 			if (neighbor.IsOccupied) continue;
	//
	// 			int moveCost = GetMovementCost(neighbor);
	// 			if (moveCost >= 9999) continue;
	//
	// 			int newCost = currentCost + moveCost;
	// 			if (newCost <= maxMovementCost)
	// 			{
	// 				if (!gCost.ContainsKey(neighbor) || newCost < gCost[neighbor])
	// 				{
	// 					gCost[neighbor] = newCost;
	// 					reachable.Add(neighbor);
	// 					openSet.Enqueue(neighbor, newCost);
	// 				}
	// 			}
	// 		}
	// 	}
	// 	return reachable;
	// }

	public bool IsCellAttackable(Vector2I originCoords, Vector2I targetCoords, int attackRange)
	{
		if (originCoords == targetCoords) return true;

		var queue = new Queue<(Vector2I, int)>();
		var visited = new HashSet<Vector2I>();

		queue.Enqueue((targetCoords, 0));
		visited.Add(targetCoords);

		while (queue.Count > 0)
		{
			var (current, dist) = queue.Dequeue();

			if (current == originCoords) return true;

			if (dist >= attackRange) continue;

			foreach (var neighbor in HexGrid.GetNeighborCoords(current))
			{
				if (visited.Contains(neighbor)) continue;

				//TODO add check for Line of Sight blockers (like Mountains) here if needed

				visited.Add(neighbor);
				queue.Enqueue((neighbor, dist + 1));
			}
		}

		return false;
	}

	public IList<Vector2I> ComputePath(Vector2I startCoords, Vector2I targetCoords)
	{
		// HexCell startCell = hexGrid.GetCell(startCoords);
		// HexCell targetCell = hexGrid.GetCell(targetCoords);
		//
		// if (startCell == null || targetCell == null) return [];

		var openSet = new PriorityQueue<Vector2I, int>();
		var closedSet = new HashSet<Vector2I>();
		var gCost = new Dictionary<Vector2I, int>();
		var parent = new Dictionary<Vector2I, Vector2I>();

		gCost[startCoords] = 0;
		int h = HexGrid.GetHexDistance(startCoords, targetCoords);
		openSet.Enqueue(startCoords, 0 + h);

		while (openSet.TryDequeue(out Vector2I current, out _))
		{
			if (current == targetCoords)
				return ReconstructPath(startCoords, targetCoords, parent);

			closedSet.Add(current);

			foreach (var neighbor in HexGrid.GetNeighborCoords(current))
			{
				if (closedSet.Contains(neighbor) || false)//TODO replace false with an IsOccupiedCall
					continue;

				// int moveCost = WorldState.Instance.TerrainState.GetMovementCost(neighbor);
				int moveCost = 1;
				if (moveCost >= 9999) continue;

				int tentativeG = gCost[current] + moveCost;

				if (tentativeG < gCost.GetValueOrDefault(neighbor, int.MaxValue))
				{
					parent[neighbor] = current;
					gCost[neighbor] = tentativeG;

					h = HexGrid.GetHexDistance(neighbor, targetCoords);
					openSet.Enqueue(neighbor, tentativeG + h);
				}
			}
		}

		return [];
	}

	private int GetMovementCost(HexCell cell)
	{
		switch (cell.Terrain)
		{
			case HexCell.TerrainType.Plains: return 1;
			case HexCell.TerrainType.Forest: return 2;
			case HexCell.TerrainType.Mountain: return 3;
			case HexCell.TerrainType.Water: return 99999;
			default: return 1;
		}
	}

	private IList<Vector2I> ReconstructPath(Vector2I start, Vector2I end, Dictionary<Vector2I, Vector2I> parentLookup)
	{
		var path = new List<Vector2I>();
		Vector2I current = end;
		while (current != start)
		{
			path.Add(current);
			current = parentLookup[current];
		}
		path.Add(start);
		path.Reverse();
		return path;
	}
}
