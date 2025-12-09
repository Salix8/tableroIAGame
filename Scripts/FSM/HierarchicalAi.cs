#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Game.AI;
using Game.State;
using Godot;

namespace Game.FSM;

[GlobalClass]
public partial class HierarchicalAi : Node, IGameStrategy
{
	[Export] InfluenceMapManager influenceMapManager;
	[Export] TroopData scoutTroop;
	[Export] TroopData knightTroop;
	[Export] TroopData archerTroop;
	StateMachine machine;
	AttackState attackState;
	SpawnState spawnState;
	ExecutionContext context;

	public void Initialize(WorldState state, PlayerId player)
	{
		context = new ExecutionContext(state, new TroopAssignmentManager(), player, influenceMapManager, scoutTroop,
			knightTroop, archerTroop);
		spawnState = new SpawnState(context);
		attackState = new AttackState(context);
		machine = new StateMachine(spawnState);
	}

	public async IAsyncEnumerable<IGameAction> GetActionGenerator(WorldState state, PlayerId player, int desiredActions,
		[EnumeratorCancellation] CancellationToken token)
	{
		for (int i = 0; i < desiredActions; i++){
			int maxIter = 10;
			influenceMapManager.UpdateMaps(state, player);
			while (true){
				maxIter--;
				if (maxIter <= 0) yield break;
				RunTransitions();


				IGameAction? res = machine.Poll();
				if (res == null) continue;
				yield return res;
				break;
			}
		}
	}

	void RunTransitions()
	{
		// --- Basic counts ---
		int selfMana = context.State.GetPlayerClaimedManaPools(context.Player).Count();
		int enemyMana = context.State.PlayerManaClaims.Count(p => p.Value != context.Player);
		int totalMana = context.State.TerrainState.TotalManaPools;
		int unclaimedMana = totalMana - (selfMana + enemyMana);

		int selfHP = context.State.GetPlayerTroops(context.Player).Sum(t => t.CurrentHealth);
		int enemyHP = context.State.GetAllTroops()
			.Where(t => t.Owner != context.Player)
			.Sum(t => t.CurrentHealth);

		int mana = context.State.GetPlayerResources(context.Player)!.Mana;
		int cheapestCost = context.ScoutTroopData.Cost;

		var allTroops = context.State.GetPlayerTroops(context.Player).ToArray();
		var unlockedTroops = allTroops.Count(troop => !context.State.LockedTroops.Contains(troop));
		var unassigned = allTroops.Where(t => !context.assignmentManager.HasAssignment(t)).ToArray();


		// ---------------------------------------
		// Hard rule: If all troops locked → spawn
		// ---------------------------------------
		if (unlockedTroops == 0){
			machine.Set(spawnState);
			GD.Print("Switch to spawn state (all troops locked)");
			return;
		}


		// --------------------------
		// Spawn Priority
		// --------------------------
		float CalcSpawnPriority()
		{
			if (mana < cheapestCost)
				return -9999f;

			float deficit = Math.Max(enemyHP - selfHP, 0f); // only if behind
			float p = deficit;

			if (unassigned.Length == 0 && mana >= cheapestCost)
				p *= 1.2f; // slight boost if no available troops

			return p;
		}

		float CalcAttackPriority()
		{
			float unclaimedRatio = totalMana > 0 ? (float)unclaimedMana / totalMana : 0f;
			float enemyRatio     = totalMana > 0 ? (float)enemyMana / totalMana : 0f;
			float selfRatio      = totalMana > 0 ? (float)selfMana / totalMana : 0f;

			float opportunity = unclaimedRatio + enemyRatio;
			float behind      = enemyRatio - selfRatio;

			float p = opportunity * 0.7f + behind * 0.3f;

			// scale with fraction of unassigned troops
			if (allTroops.Length > 0)
				p += ((float)unassigned.Length / allTroops.Length) * 2f;

			return p;
		}


		float spawnP = CalcSpawnPriority();
		float attackP = CalcAttackPriority();


		// --------------------------
		// Pick best state
		// --------------------------
		if (spawnP > attackP){
			machine.Set(spawnState);
			GD.Print($"Switch to spawn state (spawnP {spawnP} > attackP {attackP})");
		}
		else{
			machine.Set(attackState);
			GD.Print($"Switch to attack state (attackP {attackP} >= spawnP {spawnP})");
		}
	}
}