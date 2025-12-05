#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Game.AsyncEvents;
using Game.AsyncEvents.Generic;
using Godot;

namespace Game.State;

public class TroopManager
{
	readonly Dictionary<Vector2I, TroopInfo> troops = new();
	public IReadOnlyDictionary<Vector2I, TroopInfo> Troops => troops;
	readonly HashSet<TroopInfo> deadTroops = [];

	public HashSet<TroopInfo> DeadTroops => deadTroops;

	public record TroopInfo
	{
		public Vector2I Position { get; init; }
		public PlayerId Owner { get; init; }
		public TroopData Data { get; init; }
		public int CurrentHealth { get; init; }
	}

	public IEnumerable<TroopInfo> GetPlayerTroops(PlayerId id) => troops.Values.Where(troop => troop.Owner == id);

	public bool TryGetTroop(Vector2I coord, [NotNullWhen(true)] out TroopInfo? troop)
	{
		return troops.TryGetValue(coord, out troop);
	}

	public bool TryDamageTroop(TroopInfo troop, int damage, out TroopInfo damagedTroop)
	{
		Debug.Assert(Troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		troop = troop with{ CurrentHealth = Mathf.Max(troop.CurrentHealth - damage,0) };
		if (troop.CurrentHealth <= 0){
			deadTroops.Add(troop);
		}
		troops[troop.Position] = troop;
		damagedTroop = troop;
		return true;
	}

	public bool TryRemoveTroop(TroopInfo troopToRemove)
	{

		Debug.Assert(Troops.Values.Contains(troopToRemove),"Troop manager doesn't contain provided troop");
		deadTroops.Remove(troopToRemove);
		return troops.Remove(troopToRemove.Position);
	}

	public bool IsOccupied(Vector2I coord) => troops.ContainsKey(coord);

	public bool CanMoveTroop(TroopInfo troop, Vector2I to)
	{
		Debug.Assert(Troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		return !IsOccupied(to) && troops.ContainsKey(troop.Position);
	}
	public bool TryMoveTroop(TroopInfo toMove, Vector2I to, out TroopInfo movedTroop)
	{
		Debug.Assert(Troops.Values.Contains(toMove), "Troop manager doesn't contain provided troop");
		movedTroop = toMove;
		if (IsOccupied(to)) return false;
		if (!troops.Remove(toMove.Position, out TroopInfo? troop)) return false;
		movedTroop = troop with{ Position = to };
		troops[to] = movedTroop;
		return true;
	}

	public bool TryCreateTroop(TroopData data, PlayerId owner, Vector2I coord, [NotNullWhen(true)] out TroopInfo? troop)
	{
		if (IsOccupied(coord)){
			troop = null;
			return false;
		}

		troop = new TroopInfo{ CurrentHealth = data.Health, Data = data, Owner = owner, Position = coord };
		troops.Add(coord,troop);
		return true;
	}

	public Dictionary<Vector2I, HashSet<TroopInfo>> GetTroopRanges()
	{
		Dictionary<Vector2I, HashSet<TroopInfo>> ranges = new();
		foreach (TroopInfo troop in troops.Values){
			foreach (Vector2I coord in HexGrid.GetNeighbourSpiralCoords(troop.Position, troop.Data.AttackRange)){
				if (!ranges.ContainsKey(coord)){
					ranges.Add(coord, [troop]);
				}
				else{
					ranges[coord].Add(troop);
				}
			}
		}

		return ranges;
	}
}