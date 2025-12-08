using System.Collections.Generic;
using System.Linq;
using Game.State;
using Game.TroopBehaviour.Targeted;
using Godot;

namespace Game.TroopBehaviour;

public partial class TargetEnemies : BehaviourNode
{
	[Export] TargetedNodeFactory targetedNodeFactory;
	[Export] BehaviourNodeUtils.ActionOnFailure onFailure;
	public override IEnumerable<NodeEvaluation> EvaluateActions(NodeContext context)
	{
		while (true){
			HashSet<TroopManager.TroopInfo> enemies = context.State.ComputeTroopRanges()
				.GetValueOrDefault(context.Goal.Target, []);
			if (enemies.Count == 0){
				yield break;
			}
			TroopManager.TroopInfo strongestEnemy = enemies
				.Where(enemy => enemy.Owner != context.Troop.Owner)
				.MaxBy(enemy => TroopDataDangerHeuristic(enemy.Data));
			foreach (var evaluation in BehaviourNodeUtils.EvaluateNode(targetedNodeFactory.Build(strongestEnemy.Position),context, onFailure)){
				yield return evaluation;
			}
		}

	}

	static float TroopDataDangerHeuristic(TroopData data)
	{
		return data.Damage * data.AttackCount;
	}
}