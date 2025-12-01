using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.AsyncEvents;
using Game.AsyncEvents.Generic;
using Game.State;
using Godot;

namespace Game;

public class PlayerState(int playerIndex)
{
	public int PlayerIndex { get; private set; } = playerIndex;
	public int Mana { get; set; } = 0;

	readonly List<Troop> troops = [];
	public IReadOnlyList<Troop> Troops => troops;
	public async Task<bool> TryMoveTroop(Vector2I from, Vector2I to)
	{
		if (!TryGetTroop(from, out Troop troop)){
			return false;
		}

		troop.Position = to;
		await troop.MovedTo.DispatchSequential(to);
		return true;
	}

	public async Task<bool> TryDamageTroop(Vector2I position, int damage)
	{
		if (!TryGetTroop(position, out Troop troop)){
			return false;
		}

		await troop.Damage(damage);
		return true;
	}

	public bool TryGetTroop(Vector2I position, out Troop troop)
	{

		int troopIndex = troops.FindIndex(spawnedTroop => spawnedTroop.Position == position);
		if (troopIndex == -1){
			troop = null;
			return false;
		}

		troop = troops[troopIndex];
		return true;
	}

	public async Task<bool> TrySpawnTroop(TroopData data, Vector2I coord)
	{
		if (troops.Any(spawnedTroop => spawnedTroop.Position == coord)){
			return false;
		}

		Troop newTroop = new Troop(data, coord, (troop)=>troops.Remove(troop));
		troops.Add(newTroop);
		await troopSpawned.DispatchSequential(newTroop);
		return true;
	}

	readonly AsyncEvent<Troop> troopSpawned = new();
	public IAsyncHandlerCollection<Troop> TroopSpawned => troopSpawned;
	HashSet<Vector2I> claims = [];
	public event Action<Vector2I> ClaimedCoord;

	public void AddClaim(Vector2I coord)
	{
		claims.Add(coord);
		ClaimedCoord?.Invoke(coord);
	}


	public void RemoveClaim(Vector2I coord)
	{
		claims.Remove(coord);
	}
	public IReadOnlySet<Vector2I> GetClaims()
	{
		return claims;
	}

	public HashSet<Vector2I> GetSpawnableCoords()
	{
		HashSet<Vector2I> spawns = [];
		foreach (Vector2I claim in claims){
			spawns.UnionWith(HexGrid.GetNeighborCoords(claim));
		}
		return spawns;
	}

	public bool IsSpawnableCoord(Vector2I coord)
	{
		return HexGrid.GetNeighborCoords(coord).Any(claims.Contains);
	}

	public void AddMana(int amount)
	{
		Mana = Mathf.Max(0, Mana + amount);
	}

	// todo add more props
}

public class Troop(TroopData data, Vector2I startPos, Action<Troop> onKilled)
{
	public Guid Id { get; } = Guid.NewGuid(); // Add this line
	public TroopData Data => data;
	public Vector2I Position { get; set; }= startPos;
	public readonly AsyncEvent Killed = new();
	public readonly AsyncEvent<Vector2I> MovedTo = new();
	public readonly AsyncEvent<(int health, int damage)> Damaged = new();
	public int CurrentHealth { get; set; } = data.Health;

	public async Task Damage(int damage)
	{
		if (CurrentHealth == 0){
			return;
		}
		damage = Mathf.Max(CurrentHealth, damage);
		CurrentHealth -= damage;
		await Damaged.DispatchSequential((CurrentHealth, damage));
		if (CurrentHealth == 0){
			onKilled(this);
			await Killed.DispatchSequential();
		}
	}
}
