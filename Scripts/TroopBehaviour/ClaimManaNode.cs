using System.Collections.Generic;
using Game.State;
using Game.TroopBehaviour.Targeted;

namespace Game.TroopBehaviour;

public partial class ClaimManaNode : BehaviourNode
{
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		foreach (NodeEvaluation evaluation in new MoveToTargetNode(context.Goal.Target).EvaluateActions(context)){
			if (evaluation.Type == NodeEvaluation.ResultType.Failure){
				yield return new NodeEvaluation(NodeEvaluation.ResultType.Failure, null);

			}
		}

		yield return new NodeEvaluation(NodeEvaluation.ResultType.Success,
			new ClaimManaAction(context.Goal.Target, context.Troop.Owner));

	}
}