using UnityEngine;

public class StatsManager : MonoBehaviour
{
	public static StatsManager main;
	public SessionStats currentStats;

	private void Awake()
	{
		main = this;
		currentStats = new SessionStats();
	}

	public void TrackEnemyKill()
	{
		currentStats.enemiesKilled++;
	}

	public void TrackDamage(int amount)
	{
		currentStats.totalDamageDealt += amount;
	}

	public void TrackMoneyEarned(int amount)
	{
		currentStats.moneyEarned += amount;
	}

	public void TrackMoneySpent(int amount)
	{
		currentStats.moneySpent += amount;
	}

	public void TrackTowerBuilt()
	{
		currentStats.towersBuilt++;
	}

	public void TrackWave()
	{
		currentStats.wavesSurvived++;
	}
}
