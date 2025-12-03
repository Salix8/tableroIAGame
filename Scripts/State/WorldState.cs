#nullable enable
using System;
using Godot;
using Game;
using System.Collections.Generic;
using System.Diagnostics;
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
	public IAsyncHandlerCollection<TroopManager.TroopInfo> GetTroopKilledHandler();

	public IAsyncHandlerCollection<(TroopManager.TroopInfo before, TroopManager.TroopInfo after)>
		GetTroopDamagedHandler();

	public IAsyncHandlerCollection<(TroopManager.TroopInfo before, TroopManager.TroopInfo after)>
		GetTroopMovedHandler();
	public IAsyncHandlerCollection<(TroopManager.TroopInfo, Vector2I)> GetTroopAttackingHandler();
}

public class TroopEvents : ITroopEventsHandler
{
	public readonly AsyncEvent<TroopManager.TroopInfo> TroopKilled = new();
	public readonly AsyncEvent<(TroopManager.TroopInfo before,TroopManager.TroopInfo after)> TroopDamaged = new();
	public readonly AsyncEvent<(TroopManager.TroopInfo before,TroopManager.TroopInfo after)> TroopMoved = new();
	public readonly AsyncEvent<(TroopManager.TroopInfo, Vector2I)> TroopAttacking = new();
	public IAsyncHandlerCollection<TroopManager.TroopInfo>  GetTroopKilledHandler() => TroopKilled;
	public IAsyncHandlerCollection<(TroopManager.TroopInfo before,TroopManager.TroopInfo after)> GetTroopDamagedHandler() => TroopDamaged;
	public IAsyncHandlerCollection<(TroopManager.TroopInfo before,TroopManager.TroopInfo after)> GetTroopMovedHandler() => TroopMoved;
	public IAsyncHandlerCollection<(TroopManager.TroopInfo, Vector2I)> GetTroopAttackingHandler() => TroopAttacking;
}

public class WorldState
{
	public WorldState(int playerAmount)
	{
		TerrainState = new TerrainState();
		for (int i = 0; i < playerAmount; i++){
			PlayerId playerId = new(i);
			playerResources[playerId] = new PlayerResources();
		}
	}

	readonly Dictionary<PlayerId, PlayerResources> playerResources = new();

	public PlayerResources? GetPlayerResources(PlayerId id)
	{
		return playerResources.GetValueOrDefault(id);
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
	public IEnumerable<TroopManager.TroopInfo> GetPlayerTroops(PlayerId id) => troopManager.GetPlayerTroops(id);
	public async Task<bool> TryMoveTroop(TroopManager.TroopInfo troop, Vector2I to)
	{
		Vector2I from = troop.Position;
		if (!IsValidTroopCoord(to)) return false;
		if (!troopManager.CanMoveTroop(troop, to)) return false;

		var ranges = troopManager.GetTroopRanges();

		var inRange = ranges.GetValueOrDefault(from, []);
		var newRange = ranges.GetValueOrDefault(to, []);

		//opportunity attacks
		inRange.ExceptWith(newRange);
		foreach (TroopManager.TroopInfo attacker in inRange.Where(attacker => attacker.Owner != troop.Owner)){
			await TryExecuteAttack(attacker, troop, out TroopManager.TroopInfo attackedTarget);
			troop = attackedTarget;
		}

		if (!troopManager.TryMoveTroop(troop, to, out TroopManager.TroopInfo movedTroop)) return false;
		TroopEvents troopEvent = GetEventWithAssert(from);
		troopEvents.Remove(from);
		AddEventWithAssert(to, troopEvent);


		await troopEvent.TroopMoved.DispatchSequential((troop, movedTroop));

		return true;

	}
	public async Task<bool> TrySpawnTroop(TroopData data, Vector2I coord, PlayerId owner)
	{
		if (!IsValidTroopCoord(coord)) return false;
		if (!troopManager.TryCreateTroop(data, owner, coord, out TroopManager.TroopInfo? troop)) return false;
		TroopEvents events = new();
		AddEventWithAssert(coord, events);
		await troopSpawned.DispatchSequential((events, troop));

		return true;
	}
	public Task<bool> TryExecuteAttack(TroopManager.TroopInfo attacker,TroopManager.TroopInfo target, out TroopManager.TroopInfo attackedTarget)
	{
		Vector2I from = attacker.Position;
		Vector2I to = target.Position;
		TerrainState.TerrainType? type = TerrainState.GetTerrainType(to);
		attackedTarget = target;
		if (type == null){
			return Task.FromResult(false);
		}

		int damage = attacker.Data.Damage;
		damage = ModifiedDamage(type.Value, damage);
		if (!troopManager.TryDamageTroop(target, damage, out TroopManager.TroopInfo damagedTarget)) return  Task.FromResult(false);
		attackedTarget = damagedTarget;
		TroopEvents attackerEvents = GetEventWithAssert(from);
		TroopEvents targetEvents = GetEventWithAssert(to);

		TaskCompletionSource<bool> source = new();
		RunEvents();
		return source.Task;

		async void RunEvents()
		{
			await attackerEvents.TroopAttacking.DispatchSequential((attacker, target.Position));
			await targetEvents.TroopDamaged.DispatchSequential((target, damagedTarget));
			source.SetResult(true);
		}
	}
	public TroopManager.TroopInfo? GetTroop(Vector2I coord) => troopManager.TryGetTroop(coord);
	public IReadOnlyDictionary<Vector2I, TroopManager.TroopInfo> GetTroops() => troopManager.Troops;

	readonly AsyncEvent<(ITroopEventsHandler, TroopManager.TroopInfo)> troopSpawned = new();
	public IAsyncHandlerCollection<(ITroopEventsHandler, TroopManager.TroopInfo)> TroopSpawned => troopSpawned;

	public async Task KillDeadTroops()
	{
		TroopManager.TroopInfo[] deadTroops = troopManager.DeadTroops.ToArray();
		foreach (TroopManager.TroopInfo deadTroop in deadTroops){
			troopManager.TryRemoveTroop(deadTroop);
			TroopEvents events = GetEventWithAssert(deadTroop.Position);
			troopEvents.Remove(deadTroop.Position);
			await events.TroopKilled.DispatchSequential(deadTroop);
		}
	}
	public bool IsOccupied(Vector2I coord)
	{
		return TerrainState.GetTerrainType(coord) == TerrainState.TerrainType.Mountain ||
		       troopManager.IsOccupied(coord);
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

	public bool TryClaimManaPool(PlayerId playerId, Vector2I coord)
	{
		if (!IsValidPlayerId(playerId)) return false;
		TerrainState.TerrainType? terrain = TerrainState.GetTerrainType(coord);
		if (terrain is not TerrainState.TerrainType.ManaPool) return false;
		playerManaClaims[coord] = playerId;
		return true;
	}

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
		return HexGrid.GetNeighbourSpiralCoords(coord, 1).Where(IsValidTroopCoord).Any(neighbour => playerManaClaims.ContainsKey(neighbour));
	}
	bool IsValidPlayerId(PlayerId playerId)
	{
		return playerResources.ContainsKey(playerId);
	}

	public TerrainState TerrainState { get; }

	public Vector2I[] GetVisibleCoords(PlayerId playerIndex)
	{
		throw new System.NotImplementedException();
	}

	public static int ModifiedDamage(TerrainState.TerrainType type, int damage)
	{
		return type switch{
			TerrainState.TerrainType.Forest => Mathf.Max(damage - 1, 0),
			_ => damage
		};
	}
}
