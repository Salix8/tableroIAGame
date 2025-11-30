using Godot;
using Game.State;

namespace Game;

public partial class TroopSpawner : Node
{
	// [Export] PackedScene troopScene;
	// [Export] PlayableWorldState worldState;
	// [Export] HexGrid3D grid;
	//
	// public bool TrySpawnTroop(TroopData data, Vector2I coords, int playerIndex)
	// {
	// 	// New: Check if spawn location is an owned Mana Well
	// 	if (!worldState.State.ManaWells.TryGetValue(coords, out var manaWellState) || manaWellState.OwnerIndex != playerIndex)
	// 	{
	// 		GD.PrintErr($"Cannot spawn troop at {coords}: Must be an owned Mana Well.");
	// 		return false;
	// 	}
	//
	// 	PlayerState currentPlayerState = worldState.State.GetPlayerState(playerIndex);
	//
	// 	if (currentPlayerState.Mana < data.Cost)
	// 	{
	// 		GD.Print("No hay mana suficiente");
	// 		return false;
	// 	}
	//
	// 	// Check if cell is occupied
	// 	if (worldState.State.IsOccupied(coords))
	// 	{
	// 		GD.Print("Casilla ocupada");
	// 		return false;
	// 	}
	//
	// 	currentPlayerState.Mana -= data.Cost;
	//
	// 	Troop troop = troopScene.Instantiate<Troop>();
	// 	troop.Data = data;
	// 	troop.PlayerIndex = playerIndex;
	// 	troop.HexCoords = coords;
	//
	// 	Vector3 worldPos = grid.HexToWorld(coords);
	// 	troop.GlobalPosition = worldPos;
	//
	// 	AddChild(troop);
	// 	worldState.State.AddTroop(playerIndex, coords, troop);
	//
	// 	GD.Print($"Player {playerIndex} spawned {data.Name} at {coords}. Mana remaining: {currentPlayerState.Mana}");
	//
	// 	return true;
	// }
	//
	// public void TrySpawnScout(Vector2I coords)  => TrySpawnTroop(Troops.Scout, coords, worldState.CurrentPlayerIndex);
	// public void TrySpawnWarrior(Vector2I coords)=> TrySpawnTroop(Troops.Warrior, coords, worldState.CurrentPlayerIndex);
	// public void TrySpawnArcher(Vector2I coords) => TrySpawnTroop(Troops.Archer, coords, worldState.CurrentPlayerIndex);
}