#nullable enable
using Godot;
using Game.State;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Visualizers;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
    [Export] PlayerStatsUI playerStatsUI;
    [Export] Button nextTurnButton;
    [Export] Label turnInfoLabel;
    [Export] TroopSelectionMenu troopSelectionMenu;

    public override void _Ready()
    {

    }
    //todo work all the ui interaction methods in here as async methods
    public async Task<TroopData?> GetTroopSpawnSelection(PlayerResources availableResources, CancellationToken token)
    {
        troopSelectionMenu.SetEnabledTroops(troopData=> troopData.Cost <= availableResources.Mana);
        await troopSelectionMenu.ShowMenu();
        TroopData? troop = await troopSelectionMenu.GetSelection(token);
        await troopSelectionMenu.HideMenu();
        return troop;
    }
}