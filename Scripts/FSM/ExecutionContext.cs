#nullable enable
using Game.AI;
using Game.State;

namespace Game.FSM;

public class ExecutionContext(WorldState state, TroopAssignmentManager assignmentManager, PlayerId player, InfluenceMapManager influenceMapManager, TroopData scoutTroopData, TroopData knightTroopData, TroopData archerTroopData)
{
	public readonly WorldState State = state;
	public readonly TroopAssignmentManager assignmentManager = assignmentManager;
	public readonly PlayerId Player = player;
	public readonly InfluenceMapManager InfluenceMapManager = influenceMapManager;
	public readonly TroopData ScoutTroopData = scoutTroopData;
	public readonly TroopData KnightTroopData = knightTroopData;
	public readonly TroopData ArcherTroopData = archerTroopData;

}