using Godot;

namespace Game.UI;

public partial class PlayerStatsUI : Control
{
    [Export] private Label manaLabel;

    public void UpdateMana(int manaAmount)
    {
        manaLabel.Text = $"Maná: {manaAmount}";
    }
}
