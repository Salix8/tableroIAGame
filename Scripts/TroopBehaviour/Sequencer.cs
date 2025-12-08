#nullable enable
using System;
using System.Collections.Generic;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class Sequencer : BehaviourNode
{
	[Export] BehaviourNode[] sequence;
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		foreach (BehaviourNode node in sequence){
			foreach (NodeEvaluation evaluation in node.EvaluateActions(context)){
				yield return evaluation;
			}
		}
	}
}