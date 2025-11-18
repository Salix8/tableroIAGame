using Godot;

namespace Game;

public struct Troop (TroopData data, Vector2I coords, int ownerIndex)
{
	//todo refactor this
	// the troop might not need to actually hold the coordinate
    public TroopData Data { get; } = data;
    public Vector2I Coords { get; set; } = coords;
	public int Cost { get; private set; } = data.Cost;
    public int MaxHealth => Data.Health;
    public int CurrentHealth { get; private set; } = data.Health;
    public int AttackCount => Data.AttackCount;
	public int Damage => Data.Damage;
	public int AttackRange => Data.AttackRange;
	public int MovementRange => Data.MovementRange;
	public int VisionRange => Data.VisionRange;

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
}
