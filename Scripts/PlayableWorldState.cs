using System;
using System.Collections.Generic;
using Game.State;
using Game.UI;
using Godot;
using System.Threading.Tasks;

namespace Game;

[GlobalClass]
public partial class PlayableWorldState : Node
{
	[Export] HexGrid3D grid;
	readonly VersusMatch match = new(new WorldState(),3);
	public WorldState State => match.State;
	[Export] MapGenerator generator;
	[Export] TroopData testTroop;
	[Export] HumanGameStrategy human;
	[Export] GameUI gameInterface;



	public override async void _Ready()
	{
		try{
			(Dictionary<Vector2I, TerrainState.TerrainType> map, Vector2I mana1Pos, Vector2I mana2Pos) = generator.GenerateMap();
			foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighbourSpiralCoords(Vector2I.Zero,generator.MapRadius,true)){
				await State.TerrainState.AddCellAt(neighborSpiralCoord, map[neighborSpiralCoord]);
			}
			PlayerId humanPlayer = match.AddPlayer(human);
			PlayerId aiPlayer = match.AddPlayer(new RandomGameStrategy(testTroop));
			State.TryClaimManaPool(humanPlayer, mana1Pos);
			State.TryClaimManaPool(aiPlayer, mana2Pos);
			await Task.WhenAll(
				State.MutatePlayerResources(humanPlayer, _ => new PlayerResources{ Mana = 5 }),
				State.MutatePlayerResources(aiPlayer, _ => new PlayerResources{ Mana = 5 })
			);

			State.GetResourceChangedHandler(humanPlayer)?.Subscribe(gameInterface.UpdatePlayerResources);
			int curTurn = 0;
			while (curTurn < 1000){
				await match.NextTurn();
				curTurn++;
			}
		}
		catch (Exception e){
			GD.PrintErr(e);
		}
	}
}
