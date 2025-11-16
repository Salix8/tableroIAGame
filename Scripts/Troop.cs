using Godot;

namespace Game;

public partial class Troop (TroopData data, Vector2I pos, HexGrid.Entities owner) : Node3D
{
    [Export] public TroopData Data { get; set; } = data;
    public PackedScene ModelScene { get; set; } = data.ModelScene;
    public HexGrid.Entities Owner { get; set; } = owner;
    public Vector2I Coords { get; set; } = pos;
	public int Cost { get; private set; } = data.Cost;
    public int MaxHealth { get; } = data.Health;
    public int CurrentHealth { get; private set; } = data.Health;
	public int NumAttacks { get; private set; } = data.NumAttacks;
	public int Damage { get; private set; } = data.Damage;
	public int Range { get; private set; } = data.Range;
	public int Movement { get; private set; } = data.Movement;
	public int Vision { get; private set; } = data.Vision;

    public override void _Ready()
    {
        if (Data != null && Data.ModelScene != null)
        {
            var modelInstance = Data.ModelScene.Instantiate<Node3D>();
            var modelRoot = GetNode<Node3D>("ModelRoot");
            modelRoot.AddChild(modelInstance);
        }

        CurrentHealth = Data.Health;
    }

    /*
    void takeDamage(int num)
    {

    }
    void die()
    {

    }
    void select()
    {

    }
    */
}
