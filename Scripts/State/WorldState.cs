#nullable enable
using System;
using Godot;
using Game;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Game.AsyncEvents;
using Game.AsyncEvents.Generic;

namespace Game.State;

public readonly record struct PlayerId(int Value)
{
	public readonly int Value = Value;

	public override string ToString()
	{
		return $"Player {Value}";
	}
}

public interface ITroopEventsHandler
{
	public IAsyncHandlerCollection<TroopManager.IReadonlyTroopInfo> GetTroopKilledHandler();

	public IAsyncHandlerCollection<(TroopManager.TroopSnapshot before, TroopManager.IReadonlyTroopInfo after)>
		GetTroopDamagedHandler();

	public IAsyncHandlerCollection<(TroopManager.TroopSnapshot before, TroopManager.IReadonlyTroopInfo after)>
		GetTroopMovedHandler();
	public IAsyncHandlerCollection<(TroopManager.IReadonlyTroopInfo, Vector2I)> GetTroopAttackingHandler();
}

public class TroopEvents : ITroopEventsHandler
{
	public readonly AsyncEvent<TroopManager.IReadonlyTroopInfo> TroopKilled = new();
	public readonly AsyncEvent<(TroopManager.TroopSnapshot before,TroopManager.IReadonlyTroopInfo after)> TroopDamaged = new();
	public readonly AsyncEvent<(TroopManager.TroopSnapshot before,TroopManager.IReadonlyTroopInfo after)> TroopMoved = new();
	public readonly AsyncEvent<(TroopManager.IReadonlyTroopInfo, Vector2I)> TroopAttacking = new();
	public IAsyncHandlerCollection<TroopManager.IReadonlyTroopInfo>  GetTroopKilledHandler() => TroopKilled;
	public IAsyncHandlerCollection<(TroopManager.TroopSnapshot before,TroopManager.IReadonlyTroopInfo after)> GetTroopDamagedHandler() => TroopDamaged;
	public IAsyncHandlerCollection<(TroopManager.TroopSnapshot before,TroopManager.IReadonlyTroopInfo after)> GetTroopMovedHandler() => TroopMoved;
	public IAsyncHandlerCollection<(TroopManager.IReadonlyTroopInfo, Vector2I)> GetTroopAttackingHandler() => TroopAttacking;
}

public class WorldState
{
	public PlayerId RegisterNewPlayer()
	{
		int playerCount = PlayerIds.Count();
		PlayerId player = new(playerCount);
		PlayerResources playerResource = new();
		playerResources[player] = playerResource;
		resourceChangeEvents[player] = new AsyncEvent<(PlayerResources before, PlayerResources after)>();
		return player;
	}

	readonly Dictionary<PlayerId, PlayerResources> playerResources = new();
	readonly Dictionary<PlayerId, AsyncEvent<(PlayerResources before, PlayerResources after)>> resourceChangeEvents = new();

	public IAsyncHandlerCollection<(PlayerResources before, PlayerResources after)>? GetResourceChangedHandler(PlayerId id)
	{
		resourceChangeEvents.TryGetValue(id,
			out AsyncEvent<(PlayerResources before, PlayerResources after)>? value);
		return value;
	}

	public PlayerResources? GetPlayerResources(PlayerId id)
	{
		return playerResources.GetValueOrDefault(id);
	}

	public async Task MutatePlayerResources(PlayerId id, Func<PlayerResources, PlayerResources> mutator)
	{
		Debug.Assert(playerResources.ContainsKey(id), $"Player {id} is not valid!");
		PlayerResources currentResources = playerResources[id];
		Debug.Assert(resourceChangeEvents.ContainsKey(id), $"No resource change event exits for player {id}");
		PlayerResources newResources = mutator(currentResources);
		playerResources[id] = newResources;
		if (currentResources != newResources){
			await resourceChangeEvents[id].DispatchSequential((currentResources, newResources));
		}
	}


	public IEnumerable<PlayerId> PlayerIds => playerResources.Keys;

	readonly TroopManager troopManager = new();
	readonly Dictionary<Vector2I, TroopEvents> troopEvents = new();

	TroopEvents GetEventWithAssert(Vector2I coord)
	{
		troopEvents.TryGetValue(coord, out TroopEvents? value);
		Debug.Assert(value != null, $"Troop event at {coord} not found!");
		return value;
	}
	void AddEventWithAssert(Vector2I coord, TroopEvents events)
	{
		Debug.Assert(!troopEvents.ContainsKey(coord), $"A troop event already exists at {coord}");
		troopEvents.Add(coord, events);
	}
	public IEnumerable<TroopManager.IReadonlyTroopInfo> GetPlayerTroops(PlayerId id) => troopManager.GetPlayerTroops(id);

	public async Task<bool> TryMoveTroopToCell(TroopManager.IReadonlyTroopInfo troop, Vector2I to)
	{
		Vector2I from = troop.Position;
		if (!IsValidTroopCoord(to)) return false;
		if (!troopManager.CanMoveTroop(troop, to)) return false ;

		var ranges = troopManager.ComputeTroopRanges();

		var inRange = ranges.GetValueOrDefault(from, []);
		var newRange = ranges.GetValueOrDefault(to, []);

		//opportunity attacks
		inRange.ExceptWith(newRange);
		foreach (TroopManager.IReadonlyTroopInfo attacker in inRange.Where(attacker => attacker.Owner != troop.Owner)){
			await TryExecuteAttack(attacker, troop);
		}

		var beforeMove = troop.CreateSnapshot();
		if (!troopManager.TryMoveTroop(troop, to)){

			return false;
		}
		TroopEvents troopEvent = GetEventWithAssert(from);
		troopEvents.Remove(from);
		AddEventWithAssert(to, troopEvent);


		await troopEvent.TroopMoved.DispatchSequential((beforeMove, troop));

		return true;

	}

	public Dictionary<Vector2I, HashSet<TroopManager.IReadonlyTroopInfo>> ComputeTroopRanges() =>
		troopManager.ComputeTroopRanges();
	public async Task<bool> TrySpawnTroop(TroopData data, Vector2I coord, PlayerId owner)
	{
		if (!IsValidTroopCoord(coord)) return false;
		var resources = GetPlayerResources(owner);
		Debug.Assert(resources != null, $"No resources found for player {owner}");
		if (resources.Mana < data.Cost) return false;
		if (!troopManager.TryCreateTroop(data, owner, coord, out TroopManager.IReadonlyTroopInfo? troop)) return false;
		TroopEvents events = new();
		AddEventWithAssert(coord, events);
		await Task.WhenAll(
			MutatePlayerResources(owner, res => new PlayerResources{ Mana = res.Mana - data.Cost }),
			troopSpawned.DispatchSequential((events, troop))
		);

		return true;
	}
	public async  Task<bool> TryExecuteAttack(TroopManager.IReadonlyTroopInfo attacker,TroopManager.IReadonlyTroopInfo target)
	{
		Vector2I from = attacker.Position;
		Vector2I to = target.Position;
		TerrainState.TerrainType? type = TerrainState.GetTerrainType(to);
		if (type == null){
			return false;
		}

		int damage = attacker.Data.Damage;
		damage = ModifiedDamage(type.Value, damage);
		var beforeDamage = target.CreateSnapshot();
		if (!troopManager.TryDamageTroop(target, damage)) return  false;
		GD.Print($"Damaged to {target.CurrentHealth}");
		TroopEvents attackerEvents = GetEventWithAssert(from);
		TroopEvents targetEvents = GetEventWithAssert(to);

		await attackerEvents.TroopAttacking.DispatchSequential((attacker, target.Position));
		if (beforeDamage.CurrentHealth != target.CurrentHealth){
			await targetEvents.TroopDamaged.DispatchSequential((beforeDamage, target));
		}
		return true;
	}
	public bool TryGetTroop(Vector2I coord, [NotNullWhen(true)] out TroopManager.IReadonlyTroopInfo? troop) => troopManager.TryGetTroop(coord, out troop);
	public IReadOnlyDictionary<Vector2I, TroopManager.IReadonlyTroopInfo> GetTroops() => troopManager.GetTroops();

	readonly AsyncEvent<(ITroopEventsHandler, TroopManager.IReadonlyTroopInfo)> troopSpawned = new();
	public IAsyncHandlerCollection<(ITroopEventsHandler, TroopManager.IReadonlyTroopInfo)> TroopSpawned => troopSpawned;

	public async Task KillDeadTroops()
	{
		TroopManager.IReadonlyTroopInfo[] deadTroops = troopManager.DeadTroops.ToArray();
		foreach (TroopManager.IReadonlyTroopInfo deadTroop in deadTroops){
			TroopEvents events = GetEventWithAssert(deadTroop.Position);
			await events.TroopKilled.DispatchSequential(deadTroop);
			troopManager.TryRemoveTroop(deadTroop);
			troopEvents.Remove(deadTroop.Position);
		}
	}
	public bool IsValidTroopCoord(Vector2I coord)
	{
		TerrainState.TerrainType? terrain = TerrainState.GetTerrainType(coord);
		if (terrain == null){
			return false;
		}
		return terrain != TerrainState.TerrainType.Mountain && !troopManager.IsOccupied(coord) ;
	}

	readonly Dictionary<Vector2I, PlayerId> playerManaClaims = new();
	public IReadOnlyDictionary<Vector2I, PlayerId> PlayerManaClaims => playerManaClaims;
	public async Task<bool> TryClaimManaPool(PlayerId playerId, Vector2I coord)
	{
		if (!IsValidPlayerId(playerId)) return false;
		TerrainState.TerrainType? terrain = TerrainState.GetTerrainType(coord);
		if (terrain is not TerrainState.TerrainType.ManaPool) return false;
		playerManaClaims[coord] = playerId;
		await manaPoolClaimed.DispatchSequential((playerId,coord));
		return true;
	}

	AsyncEvent<(PlayerId player, Vector2I coord)> manaPoolClaimed = new();
	public IAsyncHandlerCollection<(PlayerId player, Vector2I coord)> ManaPoolClaimed => manaPoolClaimed;

	public IEnumerable<Vector2I> GetPlayerClaimedManaPools(PlayerId playerId)
	{
		return playerManaClaims.Where(pair => pair.Value == playerId).Select(pair => pair.Key);
	}


	public IEnumerable<Vector2I> GetValidPlayerSpawns(PlayerId playerId)
	{
		foreach (Vector2I playerClaimedManaPool in GetPlayerClaimedManaPools(playerId)){
			foreach (Vector2I validPos in HexGrid.GetNeighbourSpiralCoords(playerClaimedManaPool,1).Where(IsValidTroopCoord)){
				yield return validPos;
			}
		}
	}

	public bool IsValidSpawn(PlayerId playerId, Vector2I coord)
	{
		if (!IsValidTroopCoord(coord)){
			return false;
		}
		return HexGrid.GetNeighbourSpiralCoords(coord, 1).Where(IsValidTroopCoord).Any(neighbour => {
			if (!playerManaClaims.TryGetValue(neighbour, out PlayerId id)){
				return false;
			}
			return id == playerId;
		});
	}
	bool IsValidPlayerId(PlayerId playerId)
	{
		return playerResources.ContainsKey(playerId);
	}

	public TerrainState TerrainState { get; } = new();


	public static int ModifiedDamage(TerrainState.TerrainType type, int damage)
	{
		return type switch{
			TerrainState.TerrainType.Forest => Mathf.Max(damage - 1, 0),
			_ => damage
		};
	}

}
