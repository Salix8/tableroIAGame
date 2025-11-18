using Game.State;
using Godot;

namespace Game;

[GlobalClass]
public partial class PlayableWorldState : Node
{
	public readonly WorldState State = new();
}