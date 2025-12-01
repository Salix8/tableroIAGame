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
	public IGameStrategy Player1Strategy { get; private set; } // Made public property
	public IGameStrategy Player2Strategy { get; private set; } // Made public property

	public int CurrentPlayerIndex { get; private set; } = 0; // Added CurrentPlayerIndex

	public override async void _Ready()
	{
		(Dictionary<Vector2I, TerrainState.TerrainType> map, Vector2I mana1Pos, Vector2I mana2Pos) = generator.GenerateMap();
		foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighbourSpiralCoords(Vector2I.Zero,generator.MapRadius,true)){
			await State.TerrainState.AddCellAt(neighborSpiralCoord, map[neighborSpiralCoord]);
		}

		var players = State.PlayerIds.ToArray();
		GD.Print("Generation complete");
		State.TryClaimManaPool(players[0], mana1Pos);
		State.TryClaimManaPool(players[1], mana2Pos);

		Player1Strategy = new RandomGameStrategy(testTroop);
		Player2Strategy = new RandomGameStrategy(testTroop);

		// This loop simulates game turns, will need adjustment for actual human input flow
		// For now, it just sets up some initial actions.
		// The actual game loop and turn progression will need to be implemented separately.
		for (int i = 0; i < 100; i++){ // Reduced loop for initial testing
			if (i % 2 == 0){
				IGameAction action = await Player2Strategy.GetNextAction(State, players[1]);
				await action.TryApply(State);
			}
			else{
				IGameAction action = await Player1Strategy.GetNextAction(State, players[0]);
				await action.TryApply(State);
			}
		}
		// Assuming Player 0 (human) is the current player to start receiving clicks
		CurrentPlayerIndex = 0;
	}

	public void HandlePlayerClick(Vector2I hexCoord)
	{
		// Get the current player's strategy based on CurrentPlayerIndex
		IGameStrategy currentStrategy = (CurrentPlayerIndex == 0) ? Player1Strategy : Player2Strategy;

		// If the current player is human, pass the click to their strategy
		if (currentStrategy is HumanGameStrategy humanStrategy)
		{
			humanStrategy.OnHexClicked(hexCoord);
			GD.Print($"PlayableWorldState: Human player {CurrentPlayerIndex} clicked hex {hexCoord}");
			// After handling click, the game loop (elsewhere) should await the action
			// and then advance the turn.
		}
		else
		{
			GD.PrintErr($"PlayableWorldState: Non-human player {CurrentPlayerIndex} is active, click ignored.");
		}
	}
}
