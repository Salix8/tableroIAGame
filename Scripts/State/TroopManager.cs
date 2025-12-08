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
	public IReadOnlyDictionary<Vector2I, IReadonlyTroopInfo> GetTroops() => troops.ToDictionary(kvp => kvp.Key, IReadonlyTroopInfo (kvp) => kvp.Value);
	// public IReadOnlyDictionary<Vector2I, IReadonlyTroopInfo> Troops => troops.ToDictionary(kvp => kvp.Key, IReadonlyTroopInfo (kvp) => kvp.Value);

	public IEnumerable<IReadonlyTroopInfo> DeadTroops => troops.Values.Where(troop=>troop.CurrentHealth <= 0);

	public interface IReadonlyTroopInfo
	{

		public  Vector2I Position { get; }
		public PlayerId Owner { get; }
		public TroopData Data { get; }
		public int CurrentHealth { get; }

		public TroopSnapshot CreateSnapshot()
		{
			return new TroopSnapshot(){
				Position = Position,
				Owner = Owner,
				Data = Data,
				CurrentHealth = CurrentHealth
			};
		}
	}

	public record struct TroopSnapshot
	{
		public Vector2I Position;
		public PlayerId Owner;
		public TroopData Data;
		public int CurrentHealth;
	}
	class TroopInfo : IReadonlyTroopInfo
	{
		public Vector2I Position { get;set; }
		public PlayerId Owner { get; set; }
		public TroopData Data { get; set; }
		public int CurrentHealth { get; set; }

	}

	public IEnumerable<IReadonlyTroopInfo> GetPlayerTroops(PlayerId id) => troops.Values.Where(troop => troop.Owner == id);

	public bool TryGetTroop(Vector2I coord, [NotNullWhen(true)] out IReadonlyTroopInfo? troop)
	{
		var contained = troops.TryGetValue(coord, out TroopInfo? troopInfo);
		troop = troopInfo;
		return contained;
	}

	TroopInfo ConvertToWriteable(IReadonlyTroopInfo troop)
	{
		Debug.Assert(troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		return troops[troop.Position];
	}
	public bool TryDamageTroop(IReadonlyTroopInfo troop, int damage)
	{
		Debug.Assert(troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		var troopInfo = ConvertToWriteable(troop);
		troopInfo.CurrentHealth = Mathf.Max(troopInfo.CurrentHealth - damage, 0);
		return true;
	}

	public bool TryRemoveTroop(IReadonlyTroopInfo troopToRemove)
	{

		Debug.Assert(troops.Values.Contains(troopToRemove),"Troop manager doesn't contain provided troop");
		return troops.Remove(troopToRemove.Position);
	}

	public bool IsOccupied(Vector2I coord) => troops.ContainsKey(coord);

	public bool CanMoveTroop(IReadonlyTroopInfo troop, Vector2I to)
	{
		Debug.Assert(troops.Values.Contains(troop),"Troop manager doesn't contain provided troop");
		return !IsOccupied(to) && troops.ContainsKey(troop.Position);
	}
	public bool TryMoveTroop(IReadonlyTroopInfo toMove, Vector2I to)
	{
		TroopInfo troop = ConvertToWriteable(toMove);
		if (IsOccupied(to)) return false;
		if (!troops.Remove(troop.Position)) return false;
		troop.Position = to;
		troops[to] = troop;
		return true;
	}

	public bool TryCreateTroop(TroopData data, PlayerId owner, Vector2I coord, [NotNullWhen(true)] out IReadonlyTroopInfo? troop)
	{
		if (IsOccupied(coord)){
			troop = null;
			return false;
		}

		var troopInfo = new TroopInfo{ CurrentHealth = data.Health, Data = data, Owner = owner, Position = coord  };
		troop = troopInfo;
		troops.Add(coord,troopInfo);
		return true;
	}

	public Dictionary<Vector2I, HashSet<IReadonlyTroopInfo>> ComputeTroopRanges()
	{
		Dictionary<Vector2I, HashSet<IReadonlyTroopInfo>> ranges = new();
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