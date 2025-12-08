using Godot;

namespace Game;

public record struct Goal
{
	public enum GoalType
	{
		Capture,
		Reposition,
		Attack,
		Defend
	}

	public Vector2I Target;
	public GoalType Type;
}