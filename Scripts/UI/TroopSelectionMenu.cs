using Godot;
using Game.State;
using System.Linq;

namespace Game.UI
{
    public partial class TroopSelectionMenu : Control
    {

        public override void _Ready()
        {
            Visible = false;
        }
        //todo make this work with the troopDataButton

        public void ShowMenu()
        {
            if (Visible) return;

            Visible = true;
            Tween tween = CreateTween();
            float targetY = GetWindow().Size.Y - Size.Y;
            tween.TweenProperty(this, "position:y", targetY, 0.5f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.Out);
        }

        //todo rewrite this

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
            };
        }
    }
}
