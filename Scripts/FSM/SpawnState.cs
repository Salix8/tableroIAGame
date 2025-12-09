#nullable enable
using System.Linq;
using Game.State;
using Godot;

namespace Game.FSM;

public class SpawnState(ExecutionContext executionContext) : State
{
	ExecutionContext context = executionContext;

	public override void Enter()
	{
	}

	public override IGameAction? Poll()
	{
		Vector2I[] claimed = context.State.GetPlayerClaimedManaPools(context.Player).ToArray();
		if (claimed.Length == 0){
			return null;
		}

		InfluenceMap territoryMap = context.InfluenceMapManager.TerritoryMap;
		Vector2I best = claimed.MaxBy(pos => {
			float territoryVal = territoryMap.GetInfluence(pos);
			if (territoryVal < 0){
				return -10000;
			}

			return -territoryVal;
		});

		int claimedMana = context.State.GetPlayerClaimedManaPools(context.Player).Count();
		int totalClaimedMana = context.State.TerrainState.TotalManaPools;

		int mana = context.State.GetPlayerResources(context.Player)!.Mana;
		TroopData troop = ChooseTroop(ScoutProb(claimedMana, totalClaimedMana,mana),
			KnightProb(claimedMana, totalClaimedMana,mana), ArcherProb(claimedMana, totalClaimedMana,mana));
		if (troop.Cost > mana){
			return null;
		}
		foreach (Vector2I neighborCoord in HexGrid.GetNeighborCoords(best)){
			if (context.State.IsValidSpawn(context.Player, neighborCoord)){
				return new CreateTroopAction(troop, neighborCoord, context.Player);
			}
		}

		return null;
	}
	TroopData ChooseTroop(float scoutP, float knightP, float archerP)
	{
		float total = scoutP + knightP + archerP;

		if (total <= 0f)
		{
			// fallback: all zero → pick something uniform
			return context.ScoutTroopData;
		}

		float r = (float)GD.Randf() * total;

		if (r < scoutP) return context.ScoutTroopData;
		r -= scoutP;

		if (r < knightP) return context.KnightTroopData;
		r -= knightP;

		return context.ArcherTroopData;
	}
	float ScoutProb(int selfMana, int total, int manaRes)
	{
		if (manaRes < context.ScoutTroopData.Cost){
			return 0;
		}
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;

		// good when behind
		float g = (0.5f - control) * 2f;   // -1 to +1
		return Mathf.Clamp((g + 1f) * 0.5f,0,1);
	}

	float KnightProb(int selfMana, int total, int manaRes)
	{
		if (manaRes < context.KnightTroopData.Cost){
			return 0;
		}
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;

		// good when ahead
		float g = (control - 0.5f) * 2f;   // -1 to +1
		return Mathf.Clamp((g + 1f) * 0.5f,0,1);
	}

	float ArcherProb(int selfMana, int total, int manaRes)
	{
		if (manaRes < context.ArcherTroopData.Cost){
			return 0;
		}
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;

		float g = (control - 0.5f) * 2f;
		return Mathf.Clamp((g + 1f) * 0.5f,0,1);
	}

	public override void Exit()
	{
	}
}