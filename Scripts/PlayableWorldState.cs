using System; // Added for Guid
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
		foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighborSpiralCoords(Vector2I.Zero,generator.MapRadius,true)){
			await State.TerrainState.AddCellAt(neighborSpiralCoord, map[neighborSpiralCoord]);
		}
		GD.Print("Generation complete");
		State.GetPlayerState(0).AddClaim(mana1Pos);
		State.GetPlayerState(1).AddClaim(mana2Pos);

		Player1Strategy = new HumanGameStrategy(); // Player 0 is now human
		Player2Strategy = new RandomGameStrategy(testTroop); // Player 1 is AI

		// This loop simulates game turns, will need adjustment for actual human input flow
		// For now, it just sets up some initial actions.
		// The actual game loop and turn progression will need to be implemented separately.
		for (int i = 0; i < 2; i++){ // Reduced loop for initial testing
			if (i % 2 == 0){ // Player 0's turn (Human)
				// The action for the human player will come via a click, so we don't process it here.
				// For now, we'll just simulate a wait or a pass for human turn in this initial setup.
				// In a real game loop, this would await the human action.
			}
			else{ // Player 1's turn (AI)
				IGameAction action = await Player2Strategy.GetNextAction(State, 1);
				await action.TryApply(State.GetPlayerState(1), State);
			}
		}
		// Assuming Player 0 (human) is the current player to start receiving clicks
		CurrentPlayerIndex = 0;
	}

	public void HandleClickOnHex(Vector2I hexCoord)
	{
		// Get the current player's strategy based on CurrentPlayerIndex
		IGameStrategy currentStrategy = (CurrentPlayerIndex == 0) ? Player1Strategy : Player2Strategy;

		// If the current player is human, pass the click to their strategy
		if (currentStrategy is HumanGameStrategy humanStrategy)
		{
			humanStrategy.OnHexClicked(hexCoord);
			GD.Print($"PlayableWorldState: Human player {CurrentPlayerIndex} clicked hex {hexCoord}");
		}
	}

	public void HandleClickOnTroop(Troop troop)
	{
		// Get the current player's strategy based on CurrentPlayerIndex
		IGameStrategy currentStrategy = (CurrentPlayerIndex == 0) ? Player1Strategy : Player2Strategy;

		// If the current player is human, pass the click to their strategy
		if (currentStrategy is HumanGameStrategy humanStrategy)
		{
			humanStrategy.OnTroopClicked(troop);
			GD.Print($"PlayableWorldState: Human player {CurrentPlayerIndex} clicked on troop {troop.Data.Name} at {troop.Position}");
		}
	}
	
	public void HandleClickOnManaPool(Vector2I coord)
	{
		// Get the current player's strategy based on CurrentPlayerIndex
		IGameStrategy currentStrategy = (CurrentPlayerIndex == 0) ? Player1Strategy : Player2Strategy;

		// If the current player is human, pass the click to their strategy
		if (currentStrategy is HumanGameStrategy humanStrategy)
		{
			humanStrategy.OnManaPoolClicked(coord);
			GD.Print($"PlayableWorldState: Human player {CurrentPlayerIndex} clicked on Mana Pool at {coord}");
		}
	}

	public Troop GetTroopById(Guid id)
	{
		foreach (PlayerState playerState in State.PlayerStates)
		{
			foreach (Troop troop in playerState.Troops)
			{
				if (troop.Id == id)
				{
					return troop;
				}
			}
		}
		GD.PrintErr($"PlayableWorldState: Troop with ID {id} not found.");
		return null;
	}
}
