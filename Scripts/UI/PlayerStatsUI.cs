using Godot;

namespace Game.UI;

public partial class PlayerStatsUI : Control
{
    [Export] private Label _manaLabel;

    public void UpdateMana(int manaAmount)
    {
        _manaLabel.Text = $"Maná: {manaAmount}";
    }
}
