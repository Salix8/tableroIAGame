using Godot;

namespace Game.TroopBehaviour;

public record struct Goal
{
	public enum GoalType
	{
		Attack,
		Defend,
	}

	public Vector2I Target;
	public GoalType Type;
}