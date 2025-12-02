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
	readonly Dictionary<Vector2I, Troop> troops = new();
	public IReadOnlyDictionary<Vector2I, Troop> Troops => troops;
	readonly HashSet<Troop> deadTroops = [];

	public HashSet<Troop> DeadTroops => deadTroops;

	public record Troop
	{
		public Vector2I Position { get; init; }
		public PlayerId Owner { get; init; }
		public TroopData Data { get; init; }
		public int CurrentHealth { get; init; }
	}

	public IEnumerable<Troop> GetPlayerTroops(PlayerId id) => troops.Values.Where(troop => troop.Owner == id);

	public Troop? TryGetTroop(Vector2I coord)
	{
		return troops.GetValueOrDefault(coord);
	}

	public bool TryDamageTroop(ref Troop troop, int damage)
	{
		Debug.Assert(Troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		troop = troop with{ CurrentHealth = Mathf.Max(troop.CurrentHealth - damage,0) };
		if (troop.CurrentHealth <= 0){
			deadTroops.Add(troop);
		}
		troops[troop.Position] = troop;

		return true;
	}

	public bool TryRemoveTroop(Troop troopToRemove)
	{

		Debug.Assert(Troops.Values.Contains(troopToRemove),"Troop manager doesn't contain provided troop");
		deadTroops.Remove(troopToRemove);
		return troops.Remove(troopToRemove.Position);
	}

	public bool IsOccupied(Vector2I coord) => troops.ContainsKey(coord);

	public bool CanMoveTroop(Troop troop, Vector2I to)
	{
		Debug.Assert(Troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		return !IsOccupied(to) && troops.ContainsKey(troop.Position);
	}
	public bool TryMoveTroop(ref Troop movedTroop, Vector2I to)
	{
		Debug.Assert(Troops.Values.Contains(movedTroop), "Troop manager doesn't contain provided troop");
		if (IsOccupied(to)) return false;
		if (!troops.Remove(movedTroop.Position, out Troop? troop)) return false;
		troops[to] = troop with{ Position = to };
		movedTroop = movedTroop with{ Position = to };
		return true;
	}

	public bool TryCreateTroop(TroopData data, PlayerId owner, Vector2I coord, [NotNullWhen(true)] out Troop? troop)
	{
		if (IsOccupied(coord)){
			troop = null;
			return false;
		}

		troop = new Troop{ CurrentHealth = data.Health, Data = data, Owner = owner, Position = coord };
		troops.Add(coord,troop);
		return true;
	}

	public Dictionary<Vector2I, HashSet<Troop>> GetTroopRanges()
	{
		Dictionary<Vector2I, HashSet<Troop>> ranges = new();
		foreach (Troop troop in troops.Values){
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