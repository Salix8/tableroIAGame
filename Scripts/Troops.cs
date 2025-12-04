using Godot;
using System;

namespace Game;

public partial class Troops : Node
{
	[ExportSubgroup("Scout")]
	[Export] TroopData scoutData;

	[ExportSubgroup("Warrior")]
	[Export] TroopData warriorData;

	[ExportSubgroup("Archer")]
	[Export] TroopData 	archerData;

	[ExportSubgroup("Barbarian")]
	[Export] TroopData 	barbarianData;


	public static TroopData Scout { get; private set; }
	public static TroopData Warrior { get; private set; }
	public static TroopData Archer { get; private set; }
	public static TroopData Barbarian { get; private set; }
	static Troops singleton;

	public override void _EnterTree() => StoreStaticData();

	public override void _ExitTree()
	{
		if (singleton == this) singleton = null;
	}

	void StoreStaticData()
	{
		singleton ??= this;
		Scout ??= scoutData;
		Warrior ??= warriorData;
		Archer ??= archerData;
		Barbarian ??= barbarianData;
	}
}
