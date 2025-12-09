#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Game.State;
using Game.TroopBehaviour;
using Godot;

namespace Game;

public class RandomGameStrategy(TroopData troopToSpawn) : IGameStrategy
{
	public class TroopAssignmentManager
	{
		public class Assignment(
			TroopManager.IReadonlyTroopInfo troop,
			IEnumerator<NodeEvaluation> evaluator,
			Goal assignedGoal)
		{
			public readonly TroopManager.IReadonlyTroopInfo Troop = troop;
			public readonly IEnumerator<NodeEvaluation> Evaluator = evaluator;
			public readonly Goal AssignedGoal = assignedGoal;
		}

		readonly Dictionary<TroopManager.IReadonlyTroopInfo, Assignment> assignments = new();

		public void AddAssignment(NodeContext context)
		{
			var troop = context.Troop;
			if (assignments.ContainsKey(troop))
				return;

			var evaluator = context.Troop.Data.troopBehaviour
				.EvaluateActions(context)
				.GetEnumerator();

			var newAssignment = new Assignment(troop, evaluator, context.Goal);

			assignments.Add(troop, newAssignment);
		}


		// -------------------------------------------------------
		// EXECUTION (main API for strategy)
		// -------------------------------------------------------
		public IEnumerable<IGameAction> GetAssignmentActions(int maxActions, WorldState state)
		{
			if (maxActions <= 0 || assignments.Count == 0)
				yield break;

			var keys = assignments.Keys.ToArray();
			Random.Shared.Shuffle(keys);

			foreach (var troop in keys){
				if (maxActions <= 0)
					break;

				var assignment = assignments[troop];
				var evaluator = assignment.Evaluator;
				if (state.LockedTroops.Contains(troop)){
					continue;
				}
				// Dead or invalid troop
				if (troop.CurrentHealth <= 0){
					RemoveAssignment(troop);
					continue;
				}

				// If no more evaluation steps
				if (!evaluator.MoveNext()){
					RemoveAssignment(troop);
					continue;
				}

				// Process evaluation result
				NodeEvaluation eval = evaluator.Current;
				switch (eval.Type){
					case NodeEvaluation.ResultType.Failure:
						// Just remove this assignment
						RemoveAssignment(troop);
						break;

					case NodeEvaluation.ResultType.Idle:
						// Do nothing, but continue
						break;

					case NodeEvaluation.ResultType.Ongoing:
						if (eval.Result != null){
							yield return eval.Result;
							maxActions--;
						}
						break;
				}
			}
		}


		public bool RemoveAssignment(TroopManager.IReadonlyTroopInfo troop)
		{
			if (!assignments.TryGetValue(troop, out Assignment? assignment)) return false;
			assignment.Evaluator.Dispose();
			assignments.Remove(troop);
			return true;

		}


		// -------------------------------------------------------
		// UTILITY
		// -------------------------------------------------------
		public bool HasAssignment(TroopManager.IReadonlyTroopInfo troop)
		{
			return assignments.ContainsKey(troop);
		}

		public void ClearAll()
		{
			foreach (Assignment a in assignments.Values)
				a.Evaluator.Dispose();

			assignments.Clear();
		}
	}

	TroopAssignmentManager assignmentManager = new();
	// Dictionary<TroopManager.IReadonlyTroopInfo, IEnumerator<NodeEvaluation>> troopActions = new();

	public async IAsyncEnumerable<IGameAction> GetActionGenerator(WorldState state, PlayerId player, int desiredActions,
		[EnumeratorCancellation] CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		for (int actionsRan = 0; actionsRan < desiredActions; actionsRan++){
			TroopManager.IReadonlyTroopInfo[] playerTroops = state.GetPlayerTroops(player).ToArray();
			if (playerTroops.Length >= desiredActions){
				while(TryAssignTask(state, player)){}
				foreach (IGameAction gameAction in assignmentManager.GetAssignmentActions(1, state)){
					yield return gameAction;
				}


				continue;
			}

			Vector2I[] spawns = state.GetValidPlayerSpawns(player).ToArray();
			if (spawns.Length == 0){
				yield return new EmptyAction();
				continue;
			}

			Vector2I spawnPos = Random.Shared.GetItems(spawns, 1).First();
			yield return new CreateTroopAction(troopToSpawn, spawnPos, player);
		}
	}

	bool TryAssignTask(WorldState state, PlayerId player)
	{
		TroopManager.IReadonlyTroopInfo[] playerTroops = state.GetPlayerTroops(player).ToArray();
		PlayerId enemy = state.PlayerIds.First(id => id != player);
		var enemyManaPolls = state.GetPlayerClaimedManaPools(enemy).ToArray();
		if (enemyManaPolls.Length == 0){
			return false;
		}

		var randomManaPool = Random.Shared.GetItems(enemyManaPolls, 1).First();
		var unassignedTroops = playerTroops.Where(troop => !assignmentManager.HasAssignment(troop)).ToArray();
		if (unassignedTroops.Length == 0){
			return false;
		}

		TroopManager.IReadonlyTroopInfo
			randomTroop = unassignedTroops[GD.RandRange(0, unassignedTroops.Length - 1)];
		assignmentManager.AddAssignment(new NodeContext(randomTroop, new Goal(randomManaPool, Goal.GoalType.Attack), state));
		return true;
		// troopActions.Add(randomTroop,
		// 	randomTroop.Data.troopBehaviour.EvaluateActions(new NodeContext(randomTroop,
		// 		new Goal(randomManaPool, Goal.GoalType.Attack), state)).GetEnumerator());
	}


	// async IAsyncEnumerable<IGameAction> ExecuteAssignedTasks(int actionLimit)
	// {
	// 	var troopsWithGoal = troopActions.Keys.ToArray();
	// 	Random.Shared.Shuffle(troopsWithGoal);
	// 	int leftoverActions = actionLimit;
	// 	foreach (var troopWithGoal in troopsWithGoal){
	// 		var evaluator = troopActions[troopWithGoal];
	// 		if (troopWithGoal.CurrentHealth <= 0 || !evaluator.MoveNext()){
	// 			RemoveTroopTask(troopWithGoal);
	// 			yield break;
	// 		}
	//
	// 		var evaluation = evaluator.Current;
	// 		switch (evaluation.Type){
	// 			case NodeEvaluation.ResultType.Failure:
	// 				RemoveTroopTask(troopWithGoal);
	// 				break;
	// 			case NodeEvaluation.ResultType.Ongoing:
	// 				yield return evaluation.Result!;
	// 				leftoverActions--;
	// 				break;
	// 			case NodeEvaluation.ResultType.Idle:
	// 				break;
	// 			default:
	// 				throw new ArgumentOutOfRangeException();
	// 		}
	//
	// 		if (leftoverActions <= 0){
	// 			yield break;
	// 		}
	// 	}
	// }

	// void RemoveTroopTask(TroopManager.IReadonlyTroopInfo troop)
	// {
	// 	if (troopActions.Remove(troop, out IEnumerator<NodeEvaluation>? evaluator)){
	// 		evaluator.Dispose();
	// 	}
	// }
}