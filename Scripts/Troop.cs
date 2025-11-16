using Godot;

namespace Game;

public partial class Troop (TroopData data, Vector2I coords, int ownerIndex)
{
    public TroopData Data { get; set; } = data;
    public int OwnerIndex { get; set; } = ownerIndex;
    public Vector2I Coords { get; set; } = coords;
	public int Cost { get; private set; } = data.Cost;
    public int MaxHealth => data.Health;
    public int CurrentHealth { get; private set; } = data.Health;
    public int NumAttacks => data.NumAttacks;
	public int Damage => data.Damage;
	public int Range => data.Range;
	public int Movement => data.Movement;
	public int Vision => data.Vision;

    // public override void _Ready()
    // {
    //     if (Data is{ ModelScene: not null })
    //     {
    //         var modelInstance = Data.ModelScene.Instantiate<Node3D>();
    //         var modelRoot = GetNode<Node3D>("ModelRoot");
    //         modelRoot.AddChild(modelInstance);
    //     }
    //
    //     CurrentHealth = Data.Health;
    // }

    /*
     todo
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
