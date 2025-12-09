#nullable enable
using System.Linq;
using Game.State;
using Godot;

namespace Game.FSM;

public class SpawnState(ExecutionContext executionContext) : State
{
	ExecutionContext context = executionContext;

	public override void Enter() { }

	public override IGameAction? Poll()
	{
		Vector2I[] claimed = context.State.GetPlayerClaimedManaPools(context.Player).ToArray();
		if (claimed.Length == 0) return null;

		InfluenceMap territoryMap = context.InfluenceMapManager.TerritoryMap;
		Vector2I best = claimed.MaxBy(pos => {
			float territoryVal = territoryMap.GetInfluence(pos);
			return territoryVal < 0 ? -10000 : -territoryVal;
		});

		int claimedMana = context.State.GetPlayerClaimedManaPools(context.Player).Count();
		int totalMana = context.State.TerrainState.TotalManaPools;
		int mana = context.State.GetPlayerResources(context.Player)!.Mana;

		// --- Compute power bias based on troop HP ---
		int selfHP  = context.State.GetPlayerTroops(context.Player).Sum(t => t.CurrentHealth);
		int enemyHP = context.State.GetAllTroops().Where(t => t.Owner != context.Player).Sum(t => t.CurrentHealth);
		float powerBias = Mathf.Clamp((enemyHP - selfHP) / (float)Mathf.Max(enemyHP, 1), 0f, 1f); // 0..1

		TroopData troop = ChooseTroop(
			ScoutProb(claimedMana, totalMana, mana, powerBias),
			KnightProb(claimedMana, totalMana, mana, powerBias),
			ArcherProb(claimedMana, totalMana, mana, powerBias)
		);

		if (troop.Cost > mana) return null;

		foreach (Vector2I neighborCoord in HexGrid.GetNeighborCoords(best))
		{
			if (context.State.IsValidSpawn(context.Player, neighborCoord))
			{
				return new CreateTroopAction(troop, neighborCoord, context.Player);
			}
		}

		return null;
	}

	// --- Choose troop based on probability ---
	TroopData ChooseTroop(float scoutP, float knightP, float archerP)
	{
		float total = scoutP + knightP + archerP;
		if (total <= 0f) return context.ScoutTroopData;

		float r = (float)GD.Randf() * total;

		if (r < scoutP) return context.ScoutTroopData;
		r -= scoutP;
		if (r < knightP) return context.KnightTroopData;
		r -= knightP;
		return context.ArcherTroopData;
	}

	// --- Updated probabilities with power bias ---
	float ScoutProb(int selfMana, int total, int manaRes, float powerBias)
	{
		if (manaRes < context.ScoutTroopData.Cost) return 0f;
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;
		float g = (0.5f - control) * (1f - powerBias); // scouts better when ahead / low power bias
		return Mathf.Clamp((g + 1f) * 0.5f, 0f, 1f);
	}

	float KnightProb(int selfMana, int total, int manaRes, float powerBias)
	{
		if (manaRes < context.KnightTroopData.Cost) return 0f;
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;
		float g = ((control - 0.5f) + powerBias) * 0.5f; // knights scale moderately with behindness
		return Mathf.Clamp((g + 1f) * 0.5f, 0f, 1f);
	}

	float ArcherProb(int selfMana, int total, int manaRes, float powerBias)
	{
		if (manaRes < context.ArcherTroopData.Cost) return 0f;
		if (total == 0) return 0.33f;

		float control = (float)selfMana / total;
		float g = ((control - 0.5f) + powerBias) * 0.5f; // archers scale heavily when behind
		return Mathf.Clamp((g + 1f) * 0.5f, 0f, 1f);
	}

	public override void Exit() { }
}
