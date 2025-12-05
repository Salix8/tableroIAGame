using Godot;
using Game.State;
using System.Linq;
using Game.Visualizers;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
    [Export] private PlayableWorldState playableWorldState;
    [Export] private PlayerStatsUI playerStatsUI;
    [Export] private Button nextTurnButton;
    [Export] private Label turnInfoLabel;
    [Export] private TroopSelectionMenu troopSelectionMenu;
    [Export] private TileClickHandler tileClickHandler;

    public override void _Ready()
    {

    }
    //todo work all the ui interaction methods in here as async methods
}