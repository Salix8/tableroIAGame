#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Game.State;
using Game.TroopBehaviour;
using Godot;

namespace Game.FSM;

public class AttackState : State
{
	readonly ExecutionContext context;
	StateMachine machine;
	ConquerUnclaimedMana conquerUnclaimedMana;
	ConquerClaimedMana conquerClaimedMana;

	public AttackState(ExecutionContext executionContext)
	{
		context = executionContext;
		conquerUnclaimedMana = new ConquerUnclaimedMana(context);
		conquerClaimedMana = new ConquerClaimedMana(context);
		machine = new(conquerUnclaimedMana);
	}

	public override void Enter()
	{
	}

	public override IGameAction? Poll()
	{
		RunTransitions();
		return machine.Poll();
	}

	void RunTransitions()
	{
		int claimed = context.State.GetPlayerClaimedManaPools(context.Player).Count();
		int enemy = context.State.PlayerManaClaims.Count(p => p.Value != context.Player);
		int total = context.State.TerrainState.TotalManaPools;

		if (total == 0)
			return;

		int unclaimed = total - claimed - enemy;

		float priorityUnclaimed = (float)unclaimed / total;
		float priorityClaimed = enemy / (enemy + unclaimed + 0.001f);

		if (priorityClaimed > priorityUnclaimed){
			machine.Set(conquerClaimedMana);
			GD.Print("Switch to claimed mana");
		}
		else{
			machine.Set(conquerUnclaimedMana);
			GD.Print("Switch to unclaimed mana");
		}
	}

	public override void Exit()
	{
	}
}

public class ConquerUnclaimedMana(ExecutionContext context) : State
{
	public override void Enter()
	{
	}

	Vector2I[] GetUnclaimed()
	{
		IReadOnlySet<Vector2I> pools = context.State.TerrainState.ManaPools;
		var claimed = context.State.PlayerManaClaims;
		Vector2I[] unclaimed = pools.Where(pos => !claimed.ContainsKey(pos)).ToArray();
		return unclaimed;
	}

	public override IGameAction? Poll()
	{
		var unclaimed = GetUnclaimed();
		if (unclaimed.Length == 0){
			return null;
		}

		//assign all unassigned
		var unassigned = context.State.GetPlayerTroops(context.Player).Where(info => !context.assignmentManager.HasAssignment(info)).ToArray();
		InfluenceMap territoryMap = context.InfluenceMapManager.TerritoryMap;
		foreach (var troopInfo in unassigned){
			Vector2I best = unclaimed.MaxBy(pos => {
				float territoryVal = territoryMap.GetInfluence(pos);
				int distance = HexGrid.GetHexDistance(troopInfo.Position, pos);
				return territoryVal + GD.Randf() * 0.002 - distance/7f;
			});
			context.assignmentManager.AddAssignment(new NodeContext(
				troopInfo,
				new Goal(best, Goal.GoalType.Attack),
				context.State)
			);
		}


		//take all assigned evaluate which are bad and unassign them
		return context.assignmentManager.GetAssignmentActions(1, context.State, assignment => 1).FirstOrDefault();
	}


	public override void Exit()
	{
	}
}

public class ConquerClaimedMana(ExecutionContext context) : State
{
	public override void Enter()
	{
	}

	Vector2I[] GetEnemyClaimed()
	{
		var claims = context.State.PlayerManaClaims;
		return claims
			.Where(p => p.Value != context.Player) // claimed by enemy
			.Select(p => p.Key)
			.ToArray();
	}

	public override IGameAction? Poll()
	{
		var enemyClaimed = GetEnemyClaimed();
		if (enemyClaimed.Length == 0)
			return null;

		var unassigned = context.State.GetPlayerTroops(context.Player).ToArray();
		InfluenceMap territoryMap = context.InfluenceMapManager.TerritoryMap;

		foreach (var troopInfo in unassigned){
			Vector2I target = enemyClaimed.MaxBy(pos => {
				float territory = territoryMap.GetInfluence(pos);
				int distance = HexGrid.GetHexDistance(troopInfo.Position, pos);

				// We NEGATE it because positive influence = enemy control
				// so we prefer *weaker* enemy positions first.
				float desirability = territory;

				// tiny randomness so ties don't lock to same tile
				desirability += GD.Randf() * 0.05f;

				return desirability - distance/8f;
			});

			context.assignmentManager.AddAssignment(new NodeContext(
				troopInfo,
				new Goal(target, Goal.GoalType.Attack),
				context.State)
			);
		}

		//take all assigned evaluate which are bad and unassign them
		return context.assignmentManager.GetAssignmentActions(1, context.State, assignment => 1).FirstOrDefault();
	}

	public override void Exit()
	{
	}
}