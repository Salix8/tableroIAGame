using Godot;

namespace Game.UI;

public partial class GameOverScreen : Control
{
	[Export] private Label _resultLabel;
	[Export] private Button _resetButton;

	public override void _Ready()
	{
		_resetButton.Pressed += OnResetButtonPressed;
	}

	public void ShowScreen(bool playerWon)
	{
		if (playerWon)
		{
			_resultLabel.Text = "¡Has ganado!";
		}
		else
		{
			_resultLabel.Text = "Has perdido";
		}
		Visible = true;
	}

	private void OnResetButtonPressed()
	{
		// Unpause the game before reloading to avoid issues
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
