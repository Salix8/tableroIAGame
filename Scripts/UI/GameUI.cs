using Godot;
using Game.State;
using System.Linq;
using Game.Visualizers;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
    private WorldState worldState;
    [Export] private PlayableWorldState playableWorldState;
    [Export] private PlayerStatsUI playerStatsUI;
    [Export] private Button nextTurnButton;
    [Export] private Label turnInfoLabel;
    [Export] private TroopSelectionMenu troopSelectionMenu;
    [Export] private TileClickHandler tileClickHandler;

    public override void _Ready()
    {
        worldState = playableWorldState.State;
        troopSelectionMenu.WorldState = worldState;

        nextTurnButton.Pressed += OnNextTurnButtonPressed;
        worldState.TurnEnded += OnTurnEnded;
        tileClickHandler.TileClicked += OnTileClicked;
    }

    private void OnTileClicked(Vector2I coord)
    {
        var terrain = worldState.TerrainState.GetTerrainType(coord);
        if (terrain == TerrainState.TerrainType.ManaPool)
        {
            var owner = GetManaPoolOwner(coord);
            if (owner.HasValue && owner.Value == worldState.CurrentPlayerId)
            {
                troopSelectionMenu.ShowMenu(coord);
                return;
            }
        }

        troopSelectionMenu.HideMenu();
    }

    private PlayerId? GetManaPoolOwner(Vector2I coord)
    {
        foreach (var player in worldState.PlayerIds)
        {
            if (worldState.GetPlayerClaimedManaPools(player).Contains(coord))
            {
                return player;
            }
        }
        return null;
    }

    private void OnNextTurnButtonPressed()
    {
        worldState.NextTurn();
        troopSelectionMenu.HideMenu();
    }

    private void OnTurnEnded()
    {
        PlayerId playerID = worldState.CurrentPlayerId;
        turnInfoLabel.Text = $"{playerID}'s turn, Mana Pools claimed {worldState.GetPlayerClaimedManaPools(playerID).Count()} / {worldState.GetTotalManaPools()}.";
        PlayerResources? resources = worldState.GetPlayerResources(playerID);
        if(resources.HasValue)
        {
            OnPlayerResourcesChanged(resources.Value);
        }
    }

    private void OnPlayerResourcesChanged(PlayerResources resources)
    {
        playerStatsUI.UpdateMana(resources.Mana);
    }
}