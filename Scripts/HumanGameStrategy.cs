#nullable enable
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot; // Added for Vector2I
using Game.State;
using Game.Visualizers;
using Godot.Collections;

namespace Game;

[GlobalClass]
public partial class HumanGameStrategy : Node, IGameStrategy
{
	enum SelectionType
	{
		Troop,
		TroopTarget,

	}

	[Export] TileClickHandler tileClickHandler;
	[Export] HexGrid3D grid;
	TroopManager.TroopInfo? selectedTroop;
	//
	// Dictionary<(SelectionType, SelectionType), >
	public async Task<IGameAction> GetNextAction(WorldState state, PlayerId player)
	{
		Vector2I? selectedCoord = null;
		SelectionType? selectionType = null;
		IGameAction? action = null;
		while (action != null){
			(Vector2I clicked, SelectionType clickType) = await GetValidSelection(state, player);
			switch (clickType){
				case SelectionType.Troop:
					TroopManager.TroopInfo? troop = state.GetTroop(clicked);
					Debug.Assert(troop != null, $"Troop not found at {clicked}");
					if (troop.Owner != player){
						continue;
					}

					selectedTroop = troop;
					selectionType = clickType;


					break;
				case SelectionType.TroopTarget:

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		return new EmptyAction();

	}

	async Task<(Vector2I selection, SelectionType type)> GetValidSelection(WorldState state, PlayerId player)
	{
		while (true){
			(Vector2I selection, SelectionType type)? result = await SelectTile(state, player);
			if (result != null){
				return result.Value;
			}
		}
	}

	async Task<(Vector2I selection, SelectionType type)?> SelectTile(WorldState state, PlayerId player)
	{
		Vector2I clickedTile = await tileClickHandler.WaitForTileClick();
		//check contains troop
		TroopManager.TroopInfo? troop = state.GetTroop(clickedTile);
		if (troop != null && troop.Owner == player){
			return (clickedTile, SelectionType.Troop);
		}

		if (state.IsValidTroopCoord(clickedTile)){
			return (clickedTile, SelectionType.TroopTarget);
		}

		return null;
	}
}