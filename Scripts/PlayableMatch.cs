using System;
using System.Collections.Generic;
using System.Threading;
using Game.State;
using Game.UI;
using Godot;
using System.Threading.Tasks;
using Game.AI;

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
	[Export] InfluenceMapManager influenceMapManagerNode;
	Dictionary<PlayerId, PlayerInfo> players = new();
	public IReadOnlyDictionary<PlayerId, PlayerInfo> Players => players;

	CancellationToken SetupTurnCancellation()
	{
		CancellationTokenSource cancellation = new();
		gameInterface.SkipTurnButton.ButtonUp += SkipTurnButtonOnButtonUp;

		void SkipTurnButtonOnButtonUp()
		{
			cancellation.Cancel();
			gameInterface.SkipTurnButton.ButtonUp -= SkipTurnButtonOnButtonUp;
		}

		return cancellation.Token;
	}

	async Task<(PlayerId humanPlayer, PlayerId aiPlayer)> ConfigureMatch()
	{
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
		return (humanPlayer, aiPlayer);
	}
	public override async void _Ready()
	{
		try{
			(PlayerId humanPlayer, PlayerId aiPlayer) = await ConfigureMatch();
			int curTurn = 0;
			while (true){


				PlayerInfo currentPlayerInfo = players[match.CurrentPlayer];
				await gameInterface.ShowTurnText($"Turno - {currentPlayerInfo.Name}");
				CancellationToken turnCancellation = CancellationToken.None;
				if (match.CurrentPlayer == humanPlayer){
					turnCancellation = SetupTurnCancellation();
					await gameInterface.ToggleTurnControls(true);
				}
				if (match.HasLost(humanPlayer)){
					//ai wins
					break;
				}
				if (match.HasLost(aiPlayer)){
					//human wins
					break;
				}

				if(match.CurrentPlayer == aiPlayer){
					var allCoords = State.TerrainState.GetFilledPositions();
					influenceMapManagerNode.UpdateMaps(State, aiPlayer, allCoords);
				}

				try{
					await match.RunCurrentTurn(turnCancellation);
				}
				catch (OperationCanceledException){}

				await gameInterface.ToggleTurnControls(false);
				await match.RunTroopAttackPhase();
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