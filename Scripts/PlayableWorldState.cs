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
	[Export] HumanGameStrategy human;
	public PlayerId id;


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

		IGameStrategy Player1Strategy = new RandomGameStrategy(testTroop);
		IGameStrategy Player2Strategy = new RandomGameStrategy(testTroop);

		// This loop simulates game turns, will need adjustment for actual human input flow
		// For now, it just sets up some initial actions.
		// The actual game loop and turn progression will need to be implemented separately.
		for (int i = 0; i < 100; i++){ // Reduced loop for initial testing
			if (i % 10 < 5){
				IGameAction action = await Player2Strategy.GetNextAction(State, players[1]);
				await action.TryApply(State);
			}
			else{
				IGameAction action = await Player1Strategy.GetNextAction(State, players[0]);
				await action.TryApply(State);
			}
		}
	}
}
