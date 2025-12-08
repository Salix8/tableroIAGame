#nullable enable
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;

[GlobalClass]
public partial class AttackTargetTroop : TroopTargetedNodeFactory
{
	[Export] MoveToTarget move;

	public override IBehaviourNode Build(TroopManager.IReadonlyTroopInfo target)
	{
		return new AttackTroop(target,move);
	}
}
public class AttackTroop(TroopManager.IReadonlyTroopInfo target, MoveToTarget mover) : IBehaviourNode
{
	public IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		while (target.CurrentHealth > 0){
			int reach = context.Troop.Data.AttackRange;
			HashSet<Vector2I> targets = HexGrid.GetNeighbourSpiralCoords(target.Position, reach).ToHashSet();
			foreach (var evaluation in mover.Build(targets).EvaluateActions(context)){
				yield return evaluation;
			}
		}
	}
}