#nullable enable
using System.Collections.Generic;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class RepeatUntilEmpty : BehaviourNode
{
	[Export] BehaviourNode child;


	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		while (true){
			NodeEvaluation? lastEval = null;
			foreach (NodeEvaluation evaluation in child.EvaluateActions(context)){
				yield return evaluation;
				lastEval = evaluation;
			}

			if (lastEval == null) yield break;
		}
	}
}