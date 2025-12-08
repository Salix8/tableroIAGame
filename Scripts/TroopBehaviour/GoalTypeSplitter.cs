using System.Collections.Generic;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class GoalTypeSplitter : BehaviourNode
{
	[Export] Godot.Collections.Dictionary<Goal.GoalType, BehaviourNode> children;
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		if (!children.TryGetValue(context.Goal.Type, out BehaviourNode child)){
			yield return NodeEvaluation.Fail();
			yield break;
		}

		foreach (NodeEvaluation evaluation in child.EvaluateActions((context))){
			yield return evaluation;
		}
	}
}