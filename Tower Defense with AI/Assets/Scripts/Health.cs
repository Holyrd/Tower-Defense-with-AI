using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private int currencyForEnemy = 25;

	public event Action<float> OnHealthChanged;

	private bool isDestroyd = false;
	private float spawnTime;
	private int maxHitPoint;

	private void Start()
	{
		maxHitPoint = hitPoint;
		spawnTime = Time.time; 
	}

	public int GetMaxHealth()
	{
		return maxHitPoint > 0 ? maxHitPoint : hitPoint;
	}

	public void TakeDamage(int damage)
	{
		hitPoint -= damage;

		float currentPct = (float)hitPoint / maxHitPoint;
		OnHealthChanged?.Invoke(currentPct);

		StatsManager.main.TrackDamage(damage);

		if (hitPoint <= 0 && !isDestroyd)
		{
			EnemySpawner.OnEnemyDestroy.Invoke();

			int gold = Mathf.RoundToInt(currencyForEnemy * DynamicDifficultyManager.instance.currentGoldMult);
			LevelManager.main.IncreasCurrency(gold);

			StatsManager.main.TrackEnemyKill();
			StatsManager.main.TrackMoneyEarned(gold);

			isDestroyd = true;
			Destroy(gameObject);
		}
	}

	public int GiveDamage()
    {
		LevelManager.main.IncreasCurrency(currencyForEnemy);
        return hitPoint;
    }
}
