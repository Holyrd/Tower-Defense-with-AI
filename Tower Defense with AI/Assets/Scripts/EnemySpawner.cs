using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
	[Header("Enemys")]
	[SerializeField] private GameObject[] enemysPrefabs;

	[Header("Attribute")]
	[SerializeField] private int baseEnemysCount = 8;
	[SerializeField] private float enemysPerSecond = 2f;
	[SerializeField] private float diffPerLevel = 0.75f;

	[SerializeField] private int timeBeetwenWaves = 5;
	[SerializeField] private float epsCap = 15f;

	[SerializeField] private int maxWaves = 20;

	[Header("Events")]
	[SerializeField] public static UnityEvent OnEnemyDestroy = new UnityEvent();

	private float timeFromLastSpawn;
	private bool isSpawning;
	private int enemysLeftToSpawn;
	private int enemysAlive;
	private float eps; //enemyes per second
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

		if (timeFromLastSpawn >= (1f / eps) && enemysLeftToSpawn > 0)
		{
			SpawnEnemys();
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
		int index = Random.Range(0, enemysPrefabs.Length);
		GameObject prefabToSpawn = enemysPrefabs[index];
		GameObject.Instantiate(prefabToSpawn, LevelManager.main.startPoint.position, Quaternion.identity);
	}

	private void EndWave()
	{
		LevelManager.main.SetWave(++currentWave);
		isSpawning = false;
		timeFromLastSpawn = 0f;
		if (currentWave == maxWaves)
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
		isSpawning = true;
		enemysLeftToSpawn = IncreasLevelDiff();
		eps = IncreasSpeedDiff();
	}

	private int IncreasLevelDiff()
	{
		return Mathf.RoundToInt(baseEnemysCount * Mathf.Pow(currentWave, diffPerLevel));
	}

	private float IncreasSpeedDiff()
	{
		return Mathf.Clamp(enemysPerSecond * Mathf.Pow(currentWave, diffPerLevel), 0f, epsCap);
	}
}
