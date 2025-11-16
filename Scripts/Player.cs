using System.Collections.Generic;
using Godot;

namespace Game;

public abstract partial class Player : Node
{
	int mana = 0;
	Dictionary<Vector2I, Troop> troops = new();
	// todo add more props
}