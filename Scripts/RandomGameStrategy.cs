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
	Dictionary<TroopManager.IReadonlyTroopInfo, IEnumerator<NodeEvaluation>> troopActions = new();

	public async IAsyncEnumerable<IGameAction> GetActionGenerator(WorldState state, PlayerId player, int desiredActions,
		[EnumeratorCancellation] CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		for (int actionsRan = 0; actionsRan < desiredActions; actionsRan++){
			TroopManager.IReadonlyTroopInfo[] playerTroops = state.GetPlayerTroops(player).ToArray();
			if (playerTroops.Length >= desiredActions){
				await foreach (IGameAction gameAction in ExecuteAssignedTasks(1).WithCancellation(token)){
					yield return gameAction;
				}


				TryAssignTasks(state, player);
				continue;
			}

			Vector2I[] spawns = state.GetValidPlayerSpawns(player).ToArray();
			if (spawns.Length == 0){
				yield return new EmptyAction();
				continue;
			}

			Vector2I spawnPos = Random.Shared.GetItems(spawns, 1).First();
			yield return new CreateTroopAction(troopToSpawn, spawnPos, player);
			continue;
		}
	}

	void TryAssignTasks(WorldState state, PlayerId player)
	{
		TroopManager.IReadonlyTroopInfo[] playerTroops = state.GetPlayerTroops(player).ToArray();
		PlayerId enemy = state.PlayerIds.First(id => id != player);
		var enemyManaPolls = state.GetPlayerClaimedManaPools(enemy).ToArray();
		if (enemyManaPolls.Length == 0){
			return;
		}

		var randomManaPool = Random.Shared.GetItems(enemyManaPolls, 1).First();
		var untaskedTroops = playerTroops.Where(troop => !troopActions.ContainsKey(troop)).ToArray();
		if (untaskedTroops.Length == 0){
			return;
		}

		TroopManager.IReadonlyTroopInfo
			randomTroop = untaskedTroops[GD.RandRange(0, untaskedTroops.Length - 1)];
		troopActions.Add(randomTroop,
			randomTroop.Data.troopBehaviour.EvaluateActions(new NodeContext(randomTroop,
				new Goal(randomManaPool, Goal.GoalType.Attack), state)).GetEnumerator());
	}


	async IAsyncEnumerable<IGameAction> ExecuteAssignedTasks(int actionLimit)
	{
		var troopsWithGoal = troopActions.Keys.ToArray();
		Random.Shared.Shuffle(troopsWithGoal);
		int leftoverActions = actionLimit;
		foreach (var troopWithGoal in troopsWithGoal){
			var evaluator = troopActions[troopWithGoal];
			if (troopWithGoal.CurrentHealth <= 0 || !evaluator.MoveNext()){
				RemoveTroopTask(troopWithGoal);
				yield break;
			}

			var evaluation = evaluator.Current;
			switch (evaluation.Type){
				case NodeEvaluation.ResultType.Failure:
					RemoveTroopTask(troopWithGoal);
					break;
				case NodeEvaluation.ResultType.Ongoing:
					yield return evaluation.Result!;
					leftoverActions--;
					break;
				case NodeEvaluation.ResultType.Idle:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (leftoverActions <= 0){
				yield break;
			}
		}
	}

	void RemoveTroopTask(TroopManager.IReadonlyTroopInfo troop)
	{
		if (troopActions.Remove(troop, out IEnumerator<NodeEvaluation>? evaluator)){
			evaluator.Dispose();

		}



	}
}