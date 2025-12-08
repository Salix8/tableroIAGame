#nullable enable
using System.Collections.Generic;
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;

public partial class MoveToTargetFactory : TargetedNodeFactory
{
	public override IBehaviourNode Build(Vector2I target)
	{
		return new MoveToTargetNode(target);
	}
}
public class MoveToTargetNode(Vector2I target) : IBehaviourNode
{
	public IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		Vector2I[]? path = HexGridNavigation.ComputeOptimalPath(context.Troop, target, context.State);
		if (path == null){
			yield return new NodeEvaluation(NodeEvaluation.ResultType.Failure, null);
		}
		yield return new NodeEvaluation(NodeEvaluation.ResultType.Success, new MoveTroopAction(context.Troop, path));
	}
}