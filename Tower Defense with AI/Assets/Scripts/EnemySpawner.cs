using System.Collections;
using System.Collections.Generic; // ОБЯЗАТЕЛЬНО добавить для List<>
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private int timeBeetwenWaves = 5;
	[SerializeField] private int maxWaves = 20;

	[Header("Events")]
	[SerializeField] public static UnityEvent OnEnemyDestroy = new UnityEvent();

	private float timeFromLastSpawn;
	private bool isSpawning;
	private int enemysLeftToSpawn;
	private int enemysAlive;
	private float eps;
	private int currentWave;

	// НОВОЕ: Список врагов на текущую волну
	private List<GameObject> currentWaveEnemies;

	// Флаг, чтобы запустить таймер только один раз за волну
	private bool hasWaveStartedMonitoring = false;

	private void Awake()
	{
		OnEnemyDestroy.AddListener(ifDestroyEnemy);
	}

	private void Start()
	{
		StartCoroutine(StartWave());
	}

	public void Update()
	{
		if (!isSpawning) return;

		timeFromLastSpawn += Time.deltaTime;

		if (timeFromLastSpawn >= (1f / eps) && enemysLeftToSpawn > 0)
		{
			SpawnEnemys();

			// ЗАПУСК МОНИТОРИНГА (при первом спавне)
			if (!hasWaveStartedMonitoring)
			{
				PerformanceMonitor.instance.StartWaveMonitoring();
				hasWaveStartedMonitoring = true;
			}

			enemysLeftToSpawn--;
			enemysAlive++;
			timeFromLastSpawn = 0f;
		}

		if (enemysLeftToSpawn == 0 && enemysAlive == 0)
		{
			EndWave();
		}
	}

	private void ifDestroyEnemy()
	{
		enemysAlive--;
	}

	// ИСПРАВЛЕНО: Теперь берем префаб из заранее сгенерированного списка
	private void SpawnEnemys()
	{
		if (currentWaveEnemies == null || currentWaveEnemies.Count == 0) return;

		// Вычисляем индекс: Общее количество - Сколько осталось заспавнить
		// Например: Всего 10, Осталось 10 -> Индекс 0.
		//           Всего 10, Осталось 1  -> Индекс 9.
		int index = currentWaveEnemies.Count - enemysLeftToSpawn;

		// Проверка на всякий случай
		if (index >= 0 && index < currentWaveEnemies.Count)
		{
			GameObject prefabToSpawn = currentWaveEnemies[index];

			if (prefabToSpawn != null)
			{
				Instantiate(prefabToSpawn, LevelManager.main.startPoint.position, Quaternion.identity);
			}
		}
	}

	private void EndWave()
	{
		// ОСТАНОВКА МОНИТОРИНГА
		PerformanceMonitor.instance.StopWaveMonitoring();
		hasWaveStartedMonitoring = false;

		LevelManager.main.SetWave(++currentWave);
		if (StatsManager.main != null) StatsManager.main.TrackWave();

		isSpawning = false;
		timeFromLastSpawn = 0f;

		if (currentWave > maxWaves)
		{
			WinLosePanel.main.EndGame(true);
		}
		else
		{
			StartCoroutine(StartWave());
		}
	}

	private IEnumerator StartWave()
	{
		yield return new WaitForSeconds(timeBeetwenWaves);

		currentWave = LevelManager.main.wave;

		// 1. Анализ (передаем индекс прошлой волны)
		if (currentWave > 1)
		{
			// (Тут вызов метода адаптивной сложности, который мы обсуждали ранее)
			DynamicDifficultyManager.instance.EvaluateAndAdjust(currentWave - 2);
		}

		int waveIndex = currentWave - 1;

		// --- ИЗМЕНЕНИЯ ЗДЕСЬ ---
		// Получаем готовый, перемешанный список врагов (где точное количество соблюдено)
		currentWaveEnemies = DynamicDifficultyManager.instance.GetGeneratedWaveList(waveIndex);

		// Ставим количество оставшихся врагов равным длине списка
		enemysLeftToSpawn = currentWaveEnemies.Count;
		// -----------------------

		eps = DynamicDifficultyManager.instance.GetAdjustedSpawnRate(waveIndex);

		bool needPathUpdate = (currentWave == 1 || currentWave % 5 == 0);

		if (needPathUpdate)
		{
			Pathfinder.main.useDangerLogic = (currentWave >= 5);
			Pathfinder.main.UpdateDangerMap();
			Pathfinder.main.RecalculatePath();
		}

		isSpawning = true;
	}
}