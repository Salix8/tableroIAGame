using System.Collections.Generic;
using Godot;

namespace Game;

public class PlayerState
{
	public PlayerState(int playerIndex)
	{
		PlayerIndex = playerIndex;
	}

	public int PlayerIndex { get; private set; }
	public int Mana { get; set; } = 0;
	public Dictionary<Vector2I, Troop> Troops { get; set; } = new();

	public void AddMana(int amount)
	{
		Mana = Mathf.Max(0, Mana + amount);
	}

	// todo add more props
}