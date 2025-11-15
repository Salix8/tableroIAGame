using System;
using Godot;
using Godot.Collections;

namespace Game;

[GlobalClass]
public partial class TroopData : Resource
{
	[Export] public PackedScene Texture { get; private set; }
	[Export(PropertyHint.Range, positiveHint)] public int Cost { get; private set; } = 4;
	[Export(PropertyHint.Range, positiveHint)] public int Health { get; private set; } = 2;
	[Export(PropertyHint.Range, positiveHint)] public int NumAttacks { get; private set; } = 1;
	[Export(PropertyHint.Range, positiveHint)] public int Damage { get; private set; } = 2;
	[Export(PropertyHint.Range, positiveHint)] public int Range { get; private set; } = 1;
	[Export(PropertyHint.Range, positiveHint)] public int Movement { get; private set; } = 3;
	[Export(PropertyHint.Range, positiveHint)] public int Vision { get; private set; } = 5;


	const string positiveHint = "0, 1, or_greater, hide_slider";
}
