#nullable enable
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;
public partial class AttackTargetFactory : TargetedNodeFactory
{
	public override IBehaviourNode Build(Vector2I target)
	{
		return new AttackTargetNode(target);
	}
}
public class AttackTargetNode(Vector2I target) : IBehaviourNode
{
	public IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		int reach = context.Troop.Data.AttackRange;
		HashSet<Vector2I> targets = HexGrid.GetNeighbourSpiralCoords(target, reach).ToHashSet();
		Vector2I[]? path = HexGridNavigation.ComputeOptimalPath(context.Troop, targets, context.State);
		if (path == null){
			yield return new NodeEvaluation(NodeEvaluation.ResultType.Failure, null);
			yield break;
		}

		if (path.Length == 0){
			yield break;
		}
		yield return new NodeEvaluation(NodeEvaluation.ResultType.Success, new MoveTroopAction(context.Troop, path));
	}
}