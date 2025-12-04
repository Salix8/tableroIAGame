using Godot;
using Game.State;
using System.Linq;

namespace Game.UI
{
    public partial class TroopSelectionMenu : Control
    {
        public WorldState WorldState { get; set; }
        private Vector2I? selectedSpawnCoord;

        public override void _Ready()
        {
            Visible = false;
            var buttons = GetChildren().OfType<BtnTroop>();
            foreach(var btn in buttons)
            {
                btn.Pressed += () => OnTroopButtonPressed(btn);
            }
        }

        public void ShowMenu(Vector2I spawnCoord)
        {
            selectedSpawnCoord = spawnCoord;
            if (Visible) return;

            Visible = true;
            Tween tween = CreateTween();
            float targetY = GetWindow().Size.Y - Size.Y;
            tween.TweenProperty(this, "position:y", targetY, 0.5f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }

        public void HideMenu()
        {
            if (!Visible) return;

            Tween tween = CreateTween();
            float targetY = GetWindow().Size.Y;
            tween.TweenProperty(this, "position:y", targetY, 0.3f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
            tween.Finished += () => {
                Visible = false;
                selectedSpawnCoord = null;
            };
        }

        private void OnTroopButtonPressed(BtnTroop button)
        {
            if (selectedSpawnCoord.HasValue && WorldState != null)
            {
                WorldState.TrySpawnTroop(button.TroopData, selectedSpawnCoord.Value, WorldState.CurrentPlayerId);
                GD.Print($"Troop {button.TroopData.Name} requested to spawn at {selectedSpawnCoord.Value}");
            }
            HideMenu();
        }
    }
}
