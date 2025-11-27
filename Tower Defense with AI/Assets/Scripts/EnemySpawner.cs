using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private int timeBeetwenWaves = 5;
	[SerializeField] private int maxWaves = 20;

	[Header("Events")]
	[SerializeField] public static UnityEvent OnEnemyDestroy = new UnityEvent();

	// Очередь врагов на текущую волну
	private Queue<GameObject> enemiesToSpawn = new Queue<GameObject>();

	private float timeFromLastSpawn;
	private bool isSpawning;
	private int enemysAlive;
	private float currentSpawnRate;
	private int currentWave;

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

		// Спавним, пока в очереди кто-то есть
		if (timeFromLastSpawn >= (1f / currentSpawnRate) && enemiesToSpawn.Count > 0)
		{
			SpawnEnemy();
			timeFromLastSpawn = 0f;
		}

		// Если очередь пуста и на сцене никого нет — конец волны
		if (enemiesToSpawn.Count == 0 && enemysAlive == 0)
		{
			EndWave();
		}
	}

	private void SpawnEnemy()
	{
		GameObject prefab = enemiesToSpawn.Dequeue(); // Берем следующего из очереди
		Instantiate(prefab, LevelManager.main.startPoint.position, Quaternion.identity);
	}

	private void ifDestroyEnemy()
	{
		enemysAlive--;
	}

	private void EndWave()
	{
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

		// --- DDA (Сложность) ---
		if (currentWave > 1)
			DynamicDifficultyManager.instance.EvaluateAndAdjust(currentWave - 2);

		// Настройка врагов (DDA)
		List<GameObject> waveEnemies = DynamicDifficultyManager.instance.GetGeneratedWaveList(currentWave - 1);
		enemiesToSpawn.Clear();
		foreach (var enemy in waveEnemies) enemiesToSpawn.Enqueue(enemy);
		enemysAlive = waveEnemies.Count;
		currentSpawnRate = DynamicDifficultyManager.instance.GetAdjustedSpawnRate(currentWave - 1);


		// --- ЛОГИКА МАРШРУТА (SNAPSHOT) ---

		// Мы обновляем путь ТОЛЬКО:
		// 1. На самой первой волне (чтобы инициализировать)
		// 2. На каждой 5-й волне (5, 10, 15, 20...)
		bool needPathUpdate = (currentWave == 1 || currentWave % 5 == 0);

		if (needPathUpdate)
		{
			// Включаем мозги только начиная с 5-й волны
			Pathfinder.main.useDangerLogic = (currentWave >= 5);

			// Пересчитываем карту весов и сам путь
			Pathfinder.main.UpdateDangerMap();
			Pathfinder.main.RecalculatePath();

			Debug.Log($"Волна {currentWave}: Маршрут обновлен!");
		}
		else
		{
			Debug.Log($"Волна {currentWave}: Используем старый маршрут.");
		}
		// Если условие false (например, волна 6) — мы ничего не делаем.
		// В Pathfinder.main.currentWavePath лежит путь, который мы посчитали на 5-й волне.
		// Враги просто берут его и идут.

		// -----------------------------------

		isSpawning = true;
	}
}