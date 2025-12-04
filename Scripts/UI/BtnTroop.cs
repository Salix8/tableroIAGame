using Godot;

namespace Game.UI
{
    public partial class BtnTroop : Button
    {
        [Export] public TroopData TroopData { get; private set; }
    }
}