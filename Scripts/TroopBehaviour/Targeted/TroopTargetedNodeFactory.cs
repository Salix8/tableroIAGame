#nullable enable
using Game.State;
using Godot;

namespace Game.TroopBehaviour.Targeted;

[GlobalClass]
public abstract partial class TroopTargetedNodeFactory : Resource
{
	public abstract IBehaviourNode Build(TroopManager.IReadonlyTroopInfo target);
}