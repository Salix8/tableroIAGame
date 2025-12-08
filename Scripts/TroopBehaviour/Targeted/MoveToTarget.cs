#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;

[GlobalClass]
public partial class MoveToTarget : PositionTargetedNodeFactory
{
	public enum PathfindingType
	{
		MinimizedDamage,
		Weighed
	}

	[Export] OnUnreachablePathfinding onUnreachable;

	public enum OnUnreachablePathfinding
	{
		MultiStep,
		Fail
	}

	[Export] PathfindingType reachablePathfinding;
	[Export] int reachableDamageWeight;
	[Export] int reachableMovementWeight;
	[Export] PathfindingType unreachablePathfinding;
	[Export] int unreachableDamageWeight;
	[Export] int unreachableMovementWeight;

	public override IBehaviourNode Build(Vector2I target)
	{
		return Build([target]);
	}

	public IBehaviourNode Build(HashSet<Vector2I> targets)
	{
		IComparer<HexGridNavigation.Cost> reachable = reachablePathfinding switch{
			PathfindingType.MinimizedDamage => new HexGridNavigation.Cost.DamageMinimizedComparer(),
			PathfindingType.Weighed => new HexGridNavigation.Cost.WeighedComparer(reachableDamageWeight,
				reachableMovementWeight),
			_ => throw new ArgumentOutOfRangeException()
		};
		IComparer<HexGridNavigation.Cost>? unreachable = null;
		if (onUnreachable == OnUnreachablePathfinding.MultiStep){
			unreachable = reachablePathfinding switch{
				PathfindingType.MinimizedDamage => new HexGridNavigation.Cost.DamageMinimizedComparer(),
				PathfindingType.Weighed => new HexGridNavigation.Cost.WeighedComparer(unreachableDamageWeight,
					unreachableMovementWeight),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		return new MoveToTargetNode(targets, reachable, unreachable);
	}

	class MoveToTargetNode(
		HashSet<Vector2I> targets,
		IComparer<HexGridNavigation.Cost> reachablePathfinding,
		IComparer<HexGridNavigation.Cost>? unreachablePathfinding) : IBehaviourNode
	{
		public IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
		{
			while (!targets.Contains(context.Troop.Position)){
				Vector2I[]? path =
					HexGridNavigation.ComputeOptimalPath(context.Troop, targets, context.State, reachablePathfinding);
				if (path != null){
					if (path.Length == 0){
						yield break;
					}
					yield return NodeEvaluation.FromAction(new MoveTroopAction(context.Troop, path));
					yield break;
				}

				if (unreachablePathfinding == null){
					yield return NodeEvaluation.Fail();
					yield break;
				}
				Vector2I[]? uncappedPath =
					HexGridNavigation.ComputeOptimalPath(context.Troop, targets, context.State, unreachablePathfinding,
						true);
				if (uncappedPath == null){
					yield return NodeEvaluation.Fail();
					yield break;
				}

				if (uncappedPath.Length == 0){
					yield break;
				}

				int pathLength = uncappedPath.Length;
				IEnumerable<Vector2I> slice = uncappedPath.Take(Mathf.Min( context.Troop.Data.MovementRange, pathLength));
				yield return NodeEvaluation.FromAction(new MoveTroopAction(context.Troop, slice.ToArray()));

			}
		}
	}
}
