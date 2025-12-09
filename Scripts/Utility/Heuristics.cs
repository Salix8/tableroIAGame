using Game.State;

namespace Game;

public class Heuristics
{

	public static float AttackHeuristic(TroopManager.IReadonlyTroopInfo attacker, TroopManager.IReadonlyTroopInfo attacked,WorldState worldState)
	{
		if (attacked.CurrentHealth == 0)
			return -9999;

		int dmg = worldState.CalculateDamage(attacker, attacked);
		int enemyDmg = attacked.Data.Damage * attacked.Data.AttackCount;
		int enemyHp = attacked.CurrentHealth;

		if (dmg <= 0){
			return -5000;
		}

		float damageEffect = (float)dmg / (enemyHp + 1);

		bool lethal = dmg >= enemyHp;

		float killBonus = lethal ? 1000f : 0f;
		float pressure = dmg * 0.1f;
		float threatFactor = enemyDmg * 0.5f;
		float chunkBonus = damageEffect * 25f;

		float baseScore = killBonus + pressure + threatFactor + chunkBonus;

		return baseScore * damageEffect;
	}
}