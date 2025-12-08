#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.State;
using Godot;

namespace Game;

public static class HexGridNavigation
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

	public static bool IsCellAttackable(Vector2I originCoords, Vector2I targetCoords, int attackRange)
	{
		if (originCoords == targetCoords) return true;

		var queue = new Queue<(Vector2I, int)>();
		var visited = new HashSet<Vector2I>();

		queue.Enqueue((targetCoords, 0));
		visited.Add(targetCoords);

		while (queue.Count > 0){
			var (current, dist) = queue.Dequeue();

			if (current == originCoords) return true;

			if (dist >= attackRange) continue;

			foreach (var neighbor in HexGrid.GetNeighborCoords(current)){
				if (visited.Contains(neighbor)) continue;

				//TODO add check for Line of Sight blockers (like Mountains) here if needed

				visited.Add(neighbor);
				queue.Enqueue((neighbor, dist + 1));
			}
		}

		return false;
	}

	public record struct Move
	{
		public Vector2I From;
		public Vector2I To;
	}

	public record struct Node
	{
		public Vector2I Position;
		public int Damage;
	}

	public struct Cost
	{
		public int GCost;
		public int Damage;
		public int HCost;

		public class Comparer : IComparer<Cost>
		{
			public int Compare(Cost x, Cost y)
			{
				int damageComparison = x.Damage.CompareTo(y.Damage);
				if (damageComparison != 0) return damageComparison;
				return (x.GCost + x.HCost) - (y.GCost + y.HCost);
			}
		}
	}

	public static IEnumerable<(ISet<Move> targets, Move current)> Test(
		TroopManager.IReadonlyTroopInfo troop, Vector2I targetCoords, WorldState state)
	{
		var enemyRanges = state.ComputeTroopRanges();
		foreach (Vector2I position in enemyRanges.Keys){
			enemyRanges[position] = enemyRanges[position].Where(coveringTroop => coveringTroop.Owner != troop.Owner)
				.ToHashSet();
		}

		int maxMovement = troop.Data.MovementRange;

		// HexCell startCell = hexGrid.GetCell(startCoords);
		// HexCell targetCell = hexGrid.GetCell(targetCoords);
		//
		// if (startCell == null || targetCell == null) return [];
		Vector2I startCoords = troop.Position;

		var openSet = new PriorityQueue<Node, Cost>(new Cost.Comparer());
		var closedSet = new HashSet<Node>();
		var gCost = new Dictionary<Node, int>();
		var parent = new Dictionary<Node, Node>();
		Node startingNode = new Node(){
			Position = startCoords,
			Damage = 0
		};
		gCost[startingNode] = 0;

		EnqueueNeighbours(startingNode);

		while (openSet.TryDequeue(out Node currentNode, out _)){
			yield return (openSet.UnorderedItems.Select(tuple => new Move
					{ From = parent[tuple.Element].Position, To = tuple.Element.Position }).ToHashSet(),
				new Move{ From = parent[currentNode].Position, To = currentNode.Position });
			if (currentNode.Position == targetCoords){
				yield break;
			}

			closedSet.Add(currentNode);
			EnqueueNeighbours(currentNode);
		}


		void EnqueueNeighbours(Node origin)
		{
			foreach (Vector2I neighborPosition in HexGrid.GetNeighborCoords(origin.Position)){
				int moveDamage = GetDamageForMove(origin.Position, neighborPosition);
				Node neighbor = new Node(){
					Position = neighborPosition,
					Damage = origin.Damage + moveDamage
				};
				if (closedSet.Contains(neighbor) || !state.IsValidTroopCoord(neighbor.Position)){
					continue;
				}

				int? movementCost = state.TerrainState.GetMovementCostToEnter(neighbor.Position);
				Debug.Assert(movementCost != null, "Valid troop coord doesn't have terrain.");

				int tentativeG = gCost[origin] + movementCost.Value;
				int h = HexGrid.GetHexDistance(neighbor.Position, targetCoords);
				// if (tentativeG + h > maxMovement) continue; // if above movement limit skip
				if (tentativeG >= gCost.GetValueOrDefault(neighbor, int.MaxValue)) continue;
				parent[neighbor] = origin;
				gCost[neighbor] = tentativeG;
				openSet.Enqueue(neighbor, new Cost(){ GCost = tentativeG, Damage = neighbor.Damage, HCost = h });
			}
		}

		int GetDamageForMove(Vector2I from, Vector2I to)
		{
			HashSet<TroopManager.IReadonlyTroopInfo> inRange = enemyRanges.GetValueOrDefault(from, []);
			HashSet<TroopManager.IReadonlyTroopInfo> newRange = enemyRanges.GetValueOrDefault(to, []);

			IEnumerable<TroopManager.IReadonlyTroopInfo> lostRange = inRange.Where(elem => !newRange.Contains(elem));
			return lostRange.Select(enemy => enemy.Data.Damage).Sum();
		}
	}

	public static HashSet<Vector2I> ComputeReachablePositions(TroopManager.IReadonlyTroopInfo troop, WorldState state)
	{
		int maxMovement = troop.Data.MovementRange;
		var openSet = new PriorityQueue<Vector2I, int>();
		var closedSet = new HashSet<Vector2I>();
		closedSet.Add(troop.Position);
		EnqueueNeighbours(troop.Position,0);
		while (openSet.TryDequeue(out Vector2I currentNode, out int gCost)){
			closedSet.Add(currentNode);
			EnqueueNeighbours(currentNode, gCost);
		}

		return closedSet;
		void EnqueueNeighbours(Vector2I origin, int gCost)
		{
			foreach (Vector2I neighbor in HexGrid.GetNeighborCoords(origin)){

				if (closedSet.Contains(neighbor) || !state.IsValidTroopCoord(neighbor)){
					continue;
				}

				int? movementCost = state.TerrainState.GetMovementCostToEnter(neighbor);
				Debug.Assert(movementCost != null, "Valid troop coord doesn't have terrain.");

				int tentativeG = gCost + movementCost.Value;
				if (tentativeG > maxMovement) continue; // if above movement limit skip
				openSet.Enqueue(neighbor, tentativeG);
			}
		}
	}

	public static Vector2I[]? ComputeOptimalPath(TroopManager.IReadonlyTroopInfo troop, Vector2I target, WorldState state)
	{
		return ComputeOptimalPath(troop, [target], state);
	}
	public static Vector2I[]? ComputeOptimalPath(TroopManager.IReadonlyTroopInfo troop, HashSet<Vector2I> targets,
		WorldState state)
	{
		var enemyRanges = state.ComputeTroopRanges();
		foreach (Vector2I position in enemyRanges.Keys){
			enemyRanges[position] = enemyRanges[position].Where(coveringTroop => coveringTroop.Owner != troop.Owner)
				.ToHashSet();
		}

		int maxMovement = troop.Data.MovementRange;
		Vector2I startCoords = troop.Position;

		var openSet = new PriorityQueue<Node, Cost>(new Cost.Comparer());
		var closedSet = new HashSet<Node>();
		var gCost = new Dictionary<Node, int>();
		var parent = new Dictionary<Node, Node>();
		Node startingNode = new Node(){
			Position = startCoords,
			Damage = 0
		};
		gCost[startingNode] = 0;

		EnqueueNeighbours(startingNode);

		while (openSet.TryDequeue(out Node currentNode, out _)){
			if (targets.Contains(currentNode.Position)){
				return Backtrack(startingNode, currentNode);
			}

			closedSet.Add(currentNode);
			EnqueueNeighbours(currentNode);
		}

		return null;


		void EnqueueNeighbours(Node origin)
		{
			foreach (Vector2I neighborPosition in HexGrid.GetNeighborCoords(origin.Position)){
				int moveDamage = GetDamageForMove(origin.Position, neighborPosition);
				Node neighbor = new Node(){
					Position = neighborPosition,
					Damage = origin.Damage + moveDamage
				};
				if (closedSet.Contains(neighbor) || !state.IsValidTroopCoord(neighbor.Position)){
					continue;
				}
				//todo theoretically this is wrong since a node of higher damage, equal position, but larger g cost is in every way worse than the undamaged node,
				//but since these are spatially different looking them up is not easy
				//this should really cause any problems but mathematically it would be nice to discard them here too

				int? movementCost = state.TerrainState.GetMovementCostToEnter(neighbor.Position);
				Debug.Assert(movementCost != null, "Valid troop coord doesn't have terrain.");

				int tentativeG = gCost[origin] + movementCost.Value;
				int h = targets.Select(target => HexGrid.GetHexDistance(neighbor.Position, target)).Min();
				if (tentativeG + h > maxMovement) continue; // if above movement limit skip
				if (tentativeG >= gCost.GetValueOrDefault(neighbor, int.MaxValue)) continue;
				parent[neighbor] = origin;
				gCost[neighbor] = tentativeG;
				openSet.Enqueue(neighbor, new Cost(){ GCost = tentativeG, Damage = neighbor.Damage, HCost = h });
			}
		}

		int GetDamageForMove(Vector2I from, Vector2I to)
		{
			HashSet<TroopManager.IReadonlyTroopInfo> inRange = enemyRanges.GetValueOrDefault(from, []);
			HashSet<TroopManager.IReadonlyTroopInfo> newRange = enemyRanges.GetValueOrDefault(to, []);

			IEnumerable<TroopManager.IReadonlyTroopInfo> lostRange = inRange.Where(elem => !newRange.Contains(elem));
			return lostRange.Select(enemy => enemy.Data.Damage).Sum();
		}


		Vector2I[] Backtrack(Node start, Node end)
		{
			var path = new List<Node>();
			Node current = end;
			while (current != start){
				path.Add(current);
				current = parent[current];
			}

			// path.Add(start);
			path.Reverse();
			return path.Select(node=>node.Position).ToArray();
		}
	}
}