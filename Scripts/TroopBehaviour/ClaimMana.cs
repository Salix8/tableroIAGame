using System.Collections.Generic;
using Game.State;
using Game.TroopBehaviour.Targeted;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class ClaimMana : BehaviourNode
{
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		yield return NodeEvaluation.FromAction(new ClaimManaAction(context.Troop, context.Troop.Owner));

	}
}