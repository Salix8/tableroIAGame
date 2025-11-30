using System.Collections.Generic;
using Game.State;
using Game.UI;
using Godot;
using System.Linq;

namespace Game;

[GlobalClass]
public partial class PlayableWorldState : Node
{
	[Export] HexGrid3D grid;
	public readonly WorldState State = new(2);
	[Export] MapGenerator generator;
	[Export] TroopData testTroop;
	IGameStrategy player1Strategy;
	IGameStrategy player2Strategy;
	public override async void _Ready()
	{
		(Dictionary<Vector2I, TerrainState.TerrainType> map, Vector2I mana1Pos, Vector2I mana2Pos) = generator.GenerateMap();
		foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighborSpiralCoords(Vector2I.Zero,generator.MapRadius,true)){
			await State.TerrainState.AddCellAt(neighborSpiralCoord, map[neighborSpiralCoord]);
		}
		GD.Print("Generation complete");
		State.GetPlayerState(0).AddClaim(mana1Pos);
		State.GetPlayerState(1).AddClaim(mana2Pos);
		player1Strategy = new RandomGameStrategy(testTroop);
		player2Strategy = new RandomGameStrategy(testTroop);

		for (int i = 0; i < 20; i++){
			if (i % 2 == 0){
				IGameAction action = await player1Strategy.GetNextAction(State, 0);
				await action.TryApply(State.GetPlayerState(0), State);
			}
			else{
				IGameAction action = await player2Strategy.GetNextAction(State, 1);
				await action.TryApply(State.GetPlayerState(1), State);

			}
		}

	}
}
	// [Signal] public delegate void TurnEndedEventHandler(int playerIndex);
	//
	// [Export] PackedScene gameOverScreenScene;
	// GameOverScreen gameOverScreenInstance;
	//
	// public readonly WorldState State = new();
	//
	// public int CurrentPlayerIndex { get; private set; } = 0;
	// public PlayerState ActivePlayer => State.GetPlayerState(CurrentPlayerIndex);
	//
	// const int ManaPerTower = 5;
	// readonly Color[] playerColors = { Colors.Blue, Colors.Red };
	// readonly Vector3 arrowOffset = Vector3.Up * 1.5f; // Height offset for the arrow
	//
	// public override void _Ready()
	// {
	// 	gameOverScreenInstance = gameOverScreenScene.Instantiate<GameOverScreen>();
	// 	AddChild(gameOverScreenInstance);
	//
	// 	StartGame();
	// }
	//
	// public override void _Process(double delta)
	// {
	// 	// Draw ownership indicators every frame
	// 	foreach (var manaWell in State.ManaWells)
	// 	{
	// 		if (manaWell.Value.OwnerIndex.HasValue)
	// 		{
	// 			int owner = manaWell.Value.OwnerIndex.Value;
	// 			if (owner < playerColors.Length)
	// 			{
	// 				Vector3 worldPos = grid.HexToWorld(manaWell.Key);
	// 				Color playerColor = playerColors[owner];
	//
	// 				// Draw an arrow above the well
	// 				DebugDraw3D.DrawArrow(worldPos + arrowOffset, worldPos + arrowOffset + Vector3.Up, playerColor);
	// 			}
	// 		}
	// 	}
	// }
	//
	// public void StartGame()
	// {
	// 	CurrentPlayerIndex = 0;
	// 	// Additional game start logic can go here
	// }
	//
	// public void NextTurn()
	// {
	// 	// >> Part B: Process claims for the player whose turn is starting <<
	// 	// We copy to a new list to avoid modification-during-iteration issues
	// 	var claimsForCurrentPlayer = State.ConqueringClaims
	// 		.Where(claim => claim.Value == CurrentPlayerIndex)
	// 		.ToList();
	//
	// 	foreach (var claim in claimsForCurrentPlayer)
	// 	{
	// 		var wellCoords = claim.Key;
	// 		if (State.ManaWells.TryGetValue(wellCoords, out var manaWellState))
	// 		{
	// 			manaWellState.OwnerIndex = CurrentPlayerIndex;
	// 			State.ConqueringClaims.Remove(wellCoords);
	// 			GD.Print($"Player {CurrentPlayerIndex} has conquered the Mana Well at {wellCoords}!");
	// 			CheckWinLossCondition(); // Check for win/loss right after a conquest
	// 			// TODO: This is where we should lock the conquering unit from acting this turn.
	// 		}
	// 	}
	//
	// 	// Mana generation for the current player at the start of their turn
	// 	foreach (var manaWell in State.ManaWells)
	// 	{
	// 		if (manaWell.Value.OwnerIndex.HasValue && manaWell.Value.OwnerIndex.Value == CurrentPlayerIndex)
	// 		{
	// 			ActivePlayer.AddMana(ManaPerTower);
	// 			GD.Print($"Player {CurrentPlayerIndex} gained {ManaPerTower} mana from Mana Well at {manaWell.Key}. Total mana: {ActivePlayer.Mana}");
	// 		}
	// 	}
	//
	// 	// TODO: Player action phase would go here
	//
	// 	// >> Part A: Check for and create new claims at the end of the turn <<
	// 	foreach (var troop in ActivePlayer.Troops)
	// 	{
	// 		var troopCoords = troop.Key;
	// 		// Check if the troop is on a mana well
	// 		if (State.ManaWells.ContainsKey(troopCoords))
	// 		{
	// 			// Check if the well is not already owned by the current player
	// 			if (State.ManaWells[troopCoords].OwnerIndex != CurrentPlayerIndex)
	// 			{
	// 				// Add a claim for the next turn. The value is the player who is making the claim.
	// 				State.ConqueringClaims[troopCoords] = CurrentPlayerIndex;
	// 				GD.Print($"Player {CurrentPlayerIndex} is starting to conquer the Mana Well at {troopCoords}.");
	// 			}
	// 		}
	// 	}
	//
	// 	CurrentPlayerIndex = (CurrentPlayerIndex + 1) % State.players.Length;
	// 	EmitSignal(SignalName.TurnEnded, CurrentPlayerIndex);
	// }
	//
	// void CheckWinLossCondition()
	// {
	// 	// This check is very basic and assumes a 2-player game.
	// 	if (State.ManaWells.Count == 0) return; // Don't check if there are no wells on the map.
	//
	// 	int player0Wells = State.ManaWells.Values.Count(well => well.OwnerIndex == 0);
	// 	int player1Wells = State.ManaWells.Values.Count(well => well.OwnerIndex == 1);
	//
	// 	// If either player has 0 wells AND there are wells owned by the other player, the game ends.
	// 	// The second condition prevents a win on turn 1 if neutral wells exist.
	// 	if (player0Wells == 0 && player1Wells > 0)
	// 	{
	// 		// Player 0 (Human) loses
	// 		gameOverScreenInstance.ShowScreen(false);
	// 		GetTree().Paused = true;
	// 	}
	// 	else if (player1Wells == 0 && player0Wells > 0)
	// 	{
	// 		// Player 0 (Human) wins
	// 		gameOverScreenInstance.ShowScreen(true);
	// 		GetTree().Paused = true;
	// 	}
	// }
