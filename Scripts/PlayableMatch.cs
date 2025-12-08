using System;
using System.Collections.Generic;
using System.Threading;
using Game.State;
using Game.UI;
using Godot;
using System.Threading.Tasks;

namespace Game;

[GlobalClass]
public partial class PlayableMatch : Node
{
	[Export] HexGrid3D grid;
	readonly VersusMatch match = new(new WorldState(), 3);
	public WorldState State => match.State;
	[Export] MapGenerator generator;
	[Export] TroopData testTroop;
	[Export] HumanGameStrategy human;
	[Export] GameUI gameInterface;
	[Export] PlayerInfo humanPlayerInfo;
	[Export] PlayerInfo aiPlayerInfo;
	Dictionary<PlayerId, PlayerInfo> players = new();
	public IReadOnlyDictionary<PlayerId, PlayerInfo> Players => players;


	public override async void _Ready()
	{
		try{
			(Dictionary<Vector2I, TerrainState.TerrainType> map, Vector2I mana1Pos, Vector2I mana2Pos) =
				generator.GenerateMap();
			foreach (Vector2I neighborSpiralCoord in HexGrid.GetNeighbourSpiralCoords(Vector2I.Zero,
				         generator.MapRadius, true)){
				await State.TerrainState.AddCellAt(neighborSpiralCoord, map[neighborSpiralCoord]);
			}

			PlayerId humanPlayer = match.AddPlayer(human);
			PlayerId aiPlayer = match.AddPlayer(new RandomGameStrategy(testTroop));
			players = new Dictionary<PlayerId, PlayerInfo>{
				{ humanPlayer, humanPlayerInfo },
				{ aiPlayer, aiPlayerInfo }
			};
			await Task.WhenAll(
				State.TryClaimManaPool(humanPlayer, mana1Pos),
				State.TryClaimManaPool(aiPlayer, mana2Pos),
				State.MutatePlayerResources(humanPlayer, _ => new PlayerResources{ Mana = 5 }),
				State.MutatePlayerResources(aiPlayer, _ => new PlayerResources{ Mana = 5 })
			);

			State.GetResourceChangedHandler(humanPlayer)?.Subscribe(gameInterface.UpdatePlayerResources);
			int curTurn = 0;
			while (true){

				PlayerInfo currentPlayer = players[match.CurrentPlayer];
				await gameInterface.ShowTurnText($"Turno - {currentPlayer.Name}");

				if (match.HasLost(humanPlayer)){
					//human wins
					break;
				}
				if (match.HasLost(aiPlayer)){
					//ai wins
					break;
				}
				await match.RunCurrentTurn(CancellationToken.None);
				if (!match.TryAdvanceCurrentTurn()){
					//stalemate
					break;
				}


				curTurn++;
			}
		}
		catch (Exception e){
			GD.PrintErr(e);
		}
	}
}