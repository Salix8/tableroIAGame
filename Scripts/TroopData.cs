using System;
using Godot;
using Godot.Collections;

namespace Game;

[GlobalClass]
public partial class TroopData : Resource
{
	[Export] public PackedScene ModelScene { get; private set; }
	[Export] public string Name { get; private set; } = "New Troop";
	[Export(PropertyHint.Range, positiveHint)] public int Cost { get; private set; } = 4;
	[Export(PropertyHint.Range, positiveHint)] public int Health { get; private set; } = 2;
	[Export(PropertyHint.Range, positiveHint)] public int AttackCount { get; private set; } = 1;
	[Export(PropertyHint.Range, positiveHint)] public int Damage { get; private set; } = 2;
	[Export(PropertyHint.Range, positiveHint)] public int AttackRange { get; private set; } = 1;
	[Export(PropertyHint.Range, positiveHint)] public int MovementRange { get; private set; } = 3;
	[Export(PropertyHint.Range, positiveHint)] public int VisionRange { get; private set; } = 5;


	const string positiveHint = "0, 1, or_greater, hide_slider";
}
