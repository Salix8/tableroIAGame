using Godot;
using Game.State;
using System.Linq;

namespace Game.UI;

public partial class GameUI : CanvasLayer
{
    // [Export] private PlayableWorldState _worldState;
    // [Export] private PlayerStatsUI _playerStatsUI;
    // [Export] private Button _nextTurnButton;
    // [Export] private Label _turnInfoLabel;
    //
    // public override void _Ready()
    // {
    //     // Connect to signals
    //     _nextTurnButton.Pressed += OnNextTurnButtonPressed;
    //     _worldState.TurnEnded += OnTurnEnded;
    //
    //     // Initial UI update
    //     OnTurnEnded(_worldState.CurrentPlayerIndex);
    // }
    //
    // private void OnNextTurnButtonPressed()
    // {
    //     _worldState.NextTurn();
    // }
    //
    // private void OnTurnEnded(int newPlayerIndex)
    // {
    //     if (_worldState == null) return;
    //
    //     // Update the mana display for the new active player
    //     if (_playerStatsUI != null)
    //     {
    //         _playerStatsUI.UpdateMana(_worldState.ActivePlayer.Mana);
    //     }
    //
    //     // Update the turn info label
    //     if (_turnInfoLabel != null)
    //     {
    //         int wellCount = _worldState.State.ManaWells.Values.Count(well => well.OwnerIndex == newPlayerIndex);
    //         _turnInfoLabel.Text = $"Turno del Jugador {newPlayerIndex} | Zonas de Maná: {wellCount}";
    //     }
    // }
}
