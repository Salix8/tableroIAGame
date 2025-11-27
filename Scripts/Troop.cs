using Godot;

namespace Game;

public partial class Troop : Node3D
{
	[Export] public TroopData Data { get; set; }
	[Export] public Vector2I HexCoords { get; set; }
	[Export] public int PlayerIndex { get; set; }

	public int Cost => Data.Cost;
	public int MaxHealth => Data.Health;
	public int CurrentHealth { get; private set; }
	public int AttackCount => Data.AttackCount;
	public int Damage => Data.Damage;
	public int AttackRange => Data.AttackRange;
	public int MovementRange => Data.MovementRange;
	public int VisionRange => Data.VisionRange;

	public override void _Ready()
	{
		if (Data?.ModelScene != null)
		{
			var modelInstance = Data.ModelScene.Instantiate<Node3D>();
			AddChild(modelInstance); // Attach directly to Troop Node3D
		}
		CurrentHealth = Data.Health;
	}

	//todo refactor this
	// the troop might not need to actually hold the coordinate
}