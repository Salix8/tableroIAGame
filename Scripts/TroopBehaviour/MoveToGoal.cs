using System.Collections.Generic;
using Game.State;
using Game.TroopBehaviour.Targeted;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class MoveToGoal : BehaviourNode
{
	[Export] PositionTargetedNodeFactory moveToTarget;
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		foreach (NodeEvaluation evaluation in moveToTarget.Build(context.Goal.Target).EvaluateActions(context)){
			yield return evaluation;
			if (evaluation.Type == NodeEvaluation.ResultType.Failure){
				yield break;
			}

		}


	}
}