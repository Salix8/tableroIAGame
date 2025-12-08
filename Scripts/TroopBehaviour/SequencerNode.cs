#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Game.TroopBehaviour;

public partial class SequencerNode : BehaviourNode
{
	[Export] BehaviourNode[] sequence;
	[Export] BehaviourNodeUtils.ActionOnFailure actionOnFailure;

	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{

		foreach (BehaviourNode node in sequence){
			foreach (NodeEvaluation evaluation in BehaviourNodeUtils.EvaluateNode(node,context, actionOnFailure)) yield return evaluation;
		}
	}
}