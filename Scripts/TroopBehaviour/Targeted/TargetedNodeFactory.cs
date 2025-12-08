#nullable enable
using Godot;

namespace Game.TroopBehaviour.Targeted;

public abstract partial class TargetedNodeFactory : Resource
{
	public abstract IBehaviourNode Build(Vector2I target);
}