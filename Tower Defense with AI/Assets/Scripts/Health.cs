using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
	[SerializeField] private int maxHitPoint = 2;
    [SerializeField] private int currencyForEnemy = 25;

    private bool isDestroyd = false;
	private float spawnTime;

	private void Start()
	{
		maxHitPoint = hitPoint;
		spawnTime = Time.time; // Запоминаем максимум при старте
	}

	// Метод для Менеджера Сложности
	public int GetMaxHealth()
	{
		// Если мы вызываем это из префаба (где Start еще не был), вернем hitPoint
		// Если из живого врага - вернем maxHitPoint
		return maxHitPoint > 0 ? maxHitPoint : hitPoint;
	}

	public void TakeDamage(int damage)
	{
		hitPoint -= damage;

		// АГРЕГАЦИЯ: Записываем нанесенный урон
		StatsManager.main.TrackDamage(damage);

		if (PerformanceMonitor.instance)
			PerformanceMonitor.instance.RegisterDamage(damage);

		if (hitPoint <= 0 && !isDestroyd)
		{
			EnemySpawner.OnEnemyDestroy.Invoke();

			// ТУТ ЖЕ МОЖНО ВЫДАТЬ ЗОЛОТО С УЧЕТОМ БОНУСА
			int gold = Mathf.RoundToInt(currencyForEnemy * DynamicDifficultyManager.instance.currentGoldMult);
			LevelManager.main.IncreasCurrency(gold);

			// АГРЕГАЦИЯ: Записываем убийство и заработок
			StatsManager.main.TrackEnemyKill();
			StatsManager.main.TrackMoneyEarned(gold);

			isDestroyd = true;
			Destroy(gameObject);
		}
	}

	public int GiveDamage()
    {
        return hitPoint;
    }
}
