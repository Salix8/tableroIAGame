using Godot;
using Game;
using System.Collections.Generic;
using System.Linq;

public partial class GridNavigation : Node
{
	[Export] public HexGrid hexGrid;

	public HashSet<HexCell> ComputeReachableArea(Vector2I startCoords, int maxMovementCost)
	{
		var reachable = new HashSet<HexCell>();
		var openSet = new PriorityQueue<HexCell, int>();
		var gCost = new Dictionary<HexCell, int>();

		HexCell startCell = hexGrid.GetCell(startCoords);
		if (startCell == null) return reachable;

		openSet.Enqueue(startCell, 0);
		gCost[startCell] = 0;
		reachable.Add(startCell);

		while (openSet.TryDequeue(out HexCell current, out int currentCost))
		{
			if (currentCost > maxMovementCost) continue;

			foreach (var neighbor in hexGrid.GetNeighbors(current.Coords))
			{
				if (neighbor.IsOccupied) continue; 

				int moveCost = GetMovementCost(neighbor);
				if (moveCost >= 9999) continue; 

				int newCost = currentCost + moveCost;
				if (newCost <= maxMovementCost)
				{
					if (!gCost.ContainsKey(neighbor) || newCost < gCost[neighbor])
					{
						gCost[neighbor] = newCost;
						reachable.Add(neighbor);
						openSet.Enqueue(neighbor, newCost);
					}
				}
			}
		}
		return reachable; 
	}


	public HexCell[] ComputePath(Vector2I startCoords, Vector2I targetCoords)
	{
		HexCell startCell = hexGrid.GetCell(startCoords);
		HexCell targetCell = hexGrid.GetCell(targetCoords);

		if (startCell == null || targetCell == null) return [];

		var openSet = new PriorityQueue<HexCell, int>();
		var closedSet = new HashSet<HexCell>();
		var gCost = new Dictionary<HexCell, int>();
		var parent = new Dictionary<HexCell, HexCell>();

		gCost[startCell] = 0;
		int h = hexGrid.GetHexDistance(startCoords, targetCoords);
		openSet.Enqueue(startCell, 0 + h);

		while (openSet.TryDequeue(out HexCell current, out _))
		{
			if (current == targetCell)
				return ReconstructPath(startCell, targetCell, parent);

			closedSet.Add(current);

			foreach (var neighbor in hexGrid.GetNeighbors(current.Coords))
			{
				if (closedSet.Contains(neighbor) || neighbor.IsOccupied)
					continue;

				int moveCost = GetMovementCost(neighbor);
				if (moveCost >= 9999) continue;

				int tentativeG = gCost[current] + moveCost;

				if (tentativeG < gCost.GetValueOrDefault(neighbor, int.MaxValue))
				{
					parent[neighbor] = current;
					gCost[neighbor] = tentativeG;

					h = hexGrid.GetHexDistance(neighbor.Coords, targetCoords);
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
			case TerrainType.Plains: return 1;
			case TerrainType.Forest: return 2;
			case TerrainType.Mountain: return 3;
			case TerrainType.Water: return 99999;
			default: return 1;
		}
	}

	private HexCell[] ReconstructPath(HexCell start, HexCell end, Dictionary<HexCell, HexCell> parent)
	{
		var path = new List<HexCell>();
		HexCell current = end;
		while (current != start)
		{
			path.Add(current);
			current = parent[current];
		}
		path.Add(start);
		path.Reverse();
		return path.ToArray();
	}
}
