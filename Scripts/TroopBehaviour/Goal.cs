using Godot;

namespace Game.TroopBehaviour;

public record struct Goal(Vector2I target, Goal.GoalType type)
{
	public enum GoalType
	{
		Attack,
		Defend,
	}

	public Vector2I Target = target;
	public GoalType Type = type;
}