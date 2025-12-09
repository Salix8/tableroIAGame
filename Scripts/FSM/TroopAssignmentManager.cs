using System;
using System.Collections.Generic;
using System.Linq;
using Game.State;
using Game.TroopBehaviour;

namespace Game.FSM;

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

		public IEnumerable<IGameAction> GetAssignmentActions(int maxActions, WorldState state, Func<Assignment, float> assignmentPriority = null)
		{

			if (maxActions <= 0 || assignments.Count == 0)
				yield break;
			Assignment[] sortedAssignments = assignments.Values.ToArray();
			if (assignmentPriority != null){
				sortedAssignments = sortedAssignments.OrderByDescending(assignmentPriority).ToArray();
			}
			else{
				Random.Shared.Shuffle(sortedAssignments);

			}

			foreach (var assignment in sortedAssignments){
				if (maxActions <= 0)
					break;
				var troop = assignment.Troop;
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

		public Goal? GetGoal(TroopManager.IReadonlyTroopInfo troop)
		{
			return assignments.GetValueOrDefault(troop, null)?.AssignedGoal;
		}

		public void ClearAll()
		{
			foreach (Assignment a in assignments.Values)
				a.Evaluator.Dispose();

			assignments.Clear();
		}
	}