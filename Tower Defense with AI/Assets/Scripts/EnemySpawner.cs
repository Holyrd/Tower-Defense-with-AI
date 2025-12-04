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

	private float timeFromLastSpawn;
	private bool isSpawning;
	private int enemysLeftToSpawn;
	private int enemysAlive;
	private float eps;
	private int currentWave;

	private List<GameObject> currentWaveEnemies;

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

	private void SpawnEnemys()
	{
		if (currentWaveEnemies == null || currentWaveEnemies.Count == 0) return;

		int index = currentWaveEnemies.Count - enemysLeftToSpawn;

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

		if (currentWave > 1)
		{
			DynamicDifficultyManager.instance.EvaluateAndAdjust(currentWave - 2);
		}

		int waveIndex = currentWave - 1;

		currentWaveEnemies = DynamicDifficultyManager.instance.GetGeneratedWaveList(waveIndex);


		enemysLeftToSpawn = currentWaveEnemies.Count;

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