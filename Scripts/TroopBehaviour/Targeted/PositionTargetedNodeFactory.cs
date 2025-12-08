#nullable enable
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;

[GlobalClass]
public abstract partial class PositionTargetedNodeFactory : Resource
{
	public abstract IBehaviourNode Build(Vector2I target);
}