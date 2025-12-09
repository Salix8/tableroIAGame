using System.Collections.Generic;
using System.Linq;
using Game.State;
using Game.TroopBehaviour.Targeted;
using Godot;

namespace Game.TroopBehaviour;

[GlobalClass]
public partial class TargetEnemiesAtGoal : BehaviourNode
{
	[Export] TroopTargetedNodeFactory targetTroop;
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		while (true){
			HashSet<TroopManager.IReadonlyTroopInfo> enemies = context.State.ComputeTroopRanges()
				.GetValueOrDefault(context.Goal.Target, []);
			TroopManager.IReadonlyTroopInfo strongestEnemy = enemies
				.Where(enemy => enemy.Owner != context.Troop.Owner)
				.MaxBy(enemy => TroopDataDangerHeuristic(enemy.Data));
			if (strongestEnemy == null){
				yield break;
			}
			foreach (var evaluation in targetTroop.Build(strongestEnemy).EvaluateActions(context)){
				yield return evaluation;
			}
		}

	}

	static float TroopDataDangerHeuristic(TroopData data)
	{
		return data.Damage * data.AttackCount;
	}
}