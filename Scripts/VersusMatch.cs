using System.Collections.Generic;
using System.Linq;
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
		players.Add(player,strategy);
		playerOrder.Add(player);
		return player;
	}

	public async Task NextTurn()
	{
		int skip = 0;
		while (true){
			if (skip == playerOrder.Count){
				//stalemate
				return;
			}
			int turn = GetAdvancedTurn(skip);
			if (!HasLost(playerOrder[turn])){
				//valid player
				break;
			}
			skip++;
		}
		currentTurn = GetAdvancedTurn(skip);
		await Turn(playerOrder[currentTurn]);

		currentTurn = GetAdvancedTurn(1);
	}

	public PlayerId CurrentPlayer => playerOrder[currentTurn];

	int GetAdvancedTurn(int skipAmount)
	{
		return Mathf.PosMod(currentTurn + skipAmount, playerOrder.Count);
	}

	public bool HasLost(PlayerId player)
	{
		return !State.GetPlayerClaimedManaPools(player).Any();
	}

	async Task Turn(PlayerId player)
	{
		var claimedMana = State.GetPlayerClaimedManaPools(player).Count();
		await State.MutatePlayerResources(player, resources => new PlayerResources{ Mana = resources.Mana + claimedMana });
		IGameStrategy strategy = players[player];
		for (int i = 0; i < actionsPerTurn; i++){
			IGameAction action = await strategy.GetNextAction(State, player);
			await action.TryApply(State); // should kill dead troops after each step?
		}

		//run attacks for all troops
		//kill all dead troops
		await State.KillDeadTroops();
	}
}