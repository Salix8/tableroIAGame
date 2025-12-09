using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.State;
using Godot;

namespace Game;

public class VersusMatch(WorldState state, int actionsPerTurn)
{
	public readonly WorldState State = state;
	readonly Dictionary<PlayerId, IGameStrategy> players = [];
	readonly List<PlayerId> playerOrder = [];

	int currentTurn = 0;

	public PlayerId AddPlayer(IGameStrategy strategy)
	{
		PlayerId player = State.RegisterNewPlayer();
		players.Add(player, strategy);
		playerOrder.Add(player);
		return player;
	}

	public async Task NextTurn(CancellationToken token)
	{
		await Turn(playerOrder[currentTurn], token);
		currentTurn = GetAdvancedTurn(1);
	}

	public async Task RunCurrentTurn(CancellationToken token)
	{
		await Turn(playerOrder[currentTurn], token);
	}

	public bool TryAdvanceCurrentTurn()
	{
		int skip = 1;
		while (true){
			if (skip == playerOrder.Count){
				//stalemate
				return false;
			}

			int turn = GetAdvancedTurn(skip);
			if (!HasLost(playerOrder[turn])){
				currentTurn = turn;
				//valid player
				return true;
			}

			skip++;
		}
	}

	public PlayerId CurrentPlayer => playerOrder[currentTurn];

	int GetAdvancedTurn(int skipAmount)
	{
		return Mathf.PosMod(currentTurn + skipAmount, playerOrder.Count);
	}

	public bool HasLost(PlayerId player)
	{
		return !State.GetPlayerClaimedManaPools(player).Any();// && !State.GetPlayerTroops(player).Any();
	}

	async Task Turn(PlayerId player, CancellationToken token)
	{
		var claimedMana = State.GetPlayerClaimedManaPools(player).Count() * 2;
		await State.MutatePlayerResources(player,
			resources => new PlayerResources{ Mana = resources.Mana + claimedMana });
		IGameStrategy strategy = players[player];
		try{
			var generator = strategy.GetActionGenerator(State, player, actionsPerTurn, token);
			await foreach (var action in generator){
				await action.TryApply(State);
				//todo stop the generator when hit max actions
			}
			// for (int i = 0; i < actionsPerTurn; i++){
			// 	IGameAction action = generator;
			// 	await action.TryApply(State);
			// }
		}
		finally{
			//run attacks for all troops
			//kill all dead troops
			await State.KillDeadTroops();
		}
	}

	public async Task RunTroopAttackPhase()
	{
		var gameTroops = State.GetTroops();
		var attackingTroops = gameTroops.Values.ToArray();
		foreach (TroopManager.IReadonlyTroopInfo attackingTroop in attackingTroops){
			int attacks = attackingTroop.Data.AttackCount;
			var rangeCells = HexGrid.GetNeighbourSpiralCoords(attackingTroop.Position, attackingTroop.Data.AttackRange).ToArray();
			var enemiesInRange = rangeCells.Select(pos => gameTroops.GetValueOrDefault(pos, null))
				.Where(troop => troop != null && troop.Owner != attackingTroop.Owner).ToArray();
			if (enemiesInRange.Length == 0){
				continue;
			}
			for (int atck = 0; atck < attacks; atck++){
				var bestTarget = enemiesInRange.MaxBy(enemy=>Heuristics.AttackHeuristic(attackingTroop,enemy,State));
				if (bestTarget != null){
					await State.TryExecuteAttack(attackingTroop, bestTarget);
				}

			}
		}

		await State.KillDeadTroops();
	}

}