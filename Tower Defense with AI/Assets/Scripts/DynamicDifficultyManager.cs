using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Добавлено для удобной работы со списками

public class DynamicDifficultyManager : MonoBehaviour
{
	public static DynamicDifficultyManager instance;

	[Header("Файлы конфигурации")]
	public WaveConfigSO baseWaves;
	public AdjustmentRulesSO rules;
	public DifficultyProfileSO currentProfile;

	[Header("Баланс (Множители)")]
	public float currentCountMult = 1.0f;
	public float currentRateMult = 1.0f;
	public float currentGoldMult = 1.0f;

	private void Awake() { instance = this; }

	// --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

	private WaveConfigSO.WaveData GetSafeWaveData(int waveIndex)
	{
		if (baseWaves.waves.Count == 0) return new WaveConfigSO.WaveData();
		int safeIndex = Mathf.Clamp(waveIndex, 0, baseWaves.waves.Count - 1);
		return baseWaves.waves[safeIndex];
	}

	// ИСПРАВЛЕНО: Теперь берет врага из групп, так как массива possibleEnemies больше нет
	public GameObject GetEnemyPrefab(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		if (data.enemyGroups == null || data.enemyGroups.Count == 0) return null;

		// Берем случайную группу и возвращаем её префаб
		var randomGroup = data.enemyGroups[Random.Range(0, data.enemyGroups.Count)];
		return randomGroup.enemyPrefab;
	}

	// ИСПРАВЛЕНО: Считаем общее количество врагов, суммируя count из всех групп
	public int GetAdjustedEnemyCount(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		int totalBaseCount = 0;

		if (data.enemyGroups != null)
		{
			foreach (var group in data.enemyGroups)
			{
				totalBaseCount += group.count;
			}
		}

		return Mathf.RoundToInt(totalBaseCount * currentCountMult);
	}

	public float GetAdjustedSpawnRate(int waveIndex)
	{
		return GetSafeWaveData(waveIndex).spawnRate * currentRateMult * currentProfile.baseSpeedMultiplier;
	}

	// Генерация полного списка врагов для волны с учетом множителя сложности
	public List<GameObject> GetGeneratedWaveList(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		List<GameObject> finalEnemies = new List<GameObject>();

		if (data.enemyGroups != null)
		{
			// 1. Наполняем список
			foreach (var group in data.enemyGroups)
			{
				// Применяем множитель сложности к количеству
				int adjustedCount = Mathf.RoundToInt(group.count * currentCountMult);

				for (int i = 0; i < adjustedCount; i++)
				{
					finalEnemies.Add(group.enemyPrefab);
				}
			}
		}

		// 2. ПЕРЕМЕШИВАЕМ СПИСОК (Fisher-Yates Shuffle)
		for (int i = 0; i < finalEnemies.Count; i++)
		{
			GameObject temp = finalEnemies[i];
			int randomIndex = Random.Range(i, finalEnemies.Count);

			finalEnemies[i] = finalEnemies[randomIndex];
			finalEnemies[randomIndex] = temp;
		}

		return finalEnemies;
	}

	// --- ГЛАВНЫЙ МОЗГ (МАТЕМАТИКА ХП) ---

	// ИСПРАВЛЕНО: Подсчет общего ХП теперь точный, на основе групп
	private float CalculateTotalWaveHP(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		float totalHP = 0;

		if (data.enemyGroups != null)
		{
			foreach (var group in data.enemyGroups)
			{
				if (group.enemyPrefab == null) continue;

				// Получаем ХП одного врага
				float singleEnemyHP = 0;
				var hpScript = group.enemyPrefab.GetComponent<Health>();

				// Важно: GiveDamage() обычно наносит урон, но в твоем коде он возвращает hitPoint.
				// Убедись, что этот метод не "ранит" префаб, а просто возвращает значение.
				if (hpScript) singleEnemyHP = hpScript.GetMaxHealth();

				// Считаем сколько их будет с учетом множителя
				int adjustedCount = Mathf.RoundToInt(group.count * currentCountMult);

				// Добавляем к общей сумме
				totalHP += singleEnemyHP * adjustedCount;
			}
		}

		return totalHP;
	}

	// Потенциал игрока (Сумма DPS всех башен)
	private float CalculatePlayerPotentialDPS()
	{
		float totalPotential = 0f;
		// FindObjectsByType - это новый метод Unity (быстрее старого FindObjectsOfType)
		// Если у тебя старая Unity (до 2023), используй FindObjectsOfType<Turret>()
		Turret[] turrets = FindObjectsByType<Turret>(FindObjectsSortMode.None);
		foreach (var t in turrets)
		{
			totalPotential += t.GetPotentialDPS();
		}
		return totalPotential;
	}

	// --- ЛОГИКА ПРИНЯТИЯ РЕШЕНИЙ ---

	public void EvaluateAndAdjust(int waveJustFinished)
	{
		// 1. НОРМА: Какой ДПС нужен был?
		float waveHP = CalculateTotalWaveHP(waveJustFinished);
		float spawnRate = GetAdjustedSpawnRate(waveJustFinished);
		int count = GetAdjustedEnemyCount(waveJustFinished);

		// Длительность спавна (идеальная волна)
		float spawnDuration = count / Mathf.Max(spawnRate, 0.1f);

		// Требуемый ДПС, чтобы убивать в темпе спавна
		float requiredDPS = waveHP / Mathf.Max(spawnDuration, 0.1f);

		// 2. ФАКТ: Как сыграл игрок?
		PerformanceMonitor.instance.SetSpawnDuration(spawnDuration);
		float realDPS = PerformanceMonitor.instance.GetRealDPS();
		int healthLost = PerformanceMonitor.instance.HealthLostInCurrentWave;

		// 3. ПОТЕНЦИАЛ: Мог ли он сыграть лучше?
		float potentialDPS = CalculatePlayerPotentialDPS();

		Debug.Log($"DDA Report: ReqDPS={requiredDPS:F1}, RealDPS={realDPS:F1}, Potential={potentialDPS:F1}, HP Lost={healthLost}");

		// --- ПРАВИЛА ---

		// А. Игрок слишком крут (Реальный ДПС > Требуемого + порог)
		if (realDPS > requiredDPS * rules.highPerformanceThreshold)
		{
			currentCountMult *= rules.enemyCountMultiplier_Harder;
			currentRateMult *= rules.spawnRateMultiplier_Harder;
			currentGoldMult *= rules.goldRewardMultiplier_Bonus;
			Debug.Log(">> Игрок доминирует -> Усложняем.");
		}
		// Б. Игрок слаб (Реальный ДПС ниже нормы)
		else if (realDPS < requiredDPS * rules.lowPerformanceThreshold)
		{
			// Проверка 1: Потерял ли он ХП?
			if (healthLost > 0)
			{
				currentCountMult *= rules.enemyCountMultiplier_Easier;
				currentRateMult *= rules.spawnRateMultiplier_Easier;
				currentGoldMult = 1.0f;
				Debug.Log(">> Не хватает мощи и есть потери -> Облегчаем.");
			}
			else
			{
				Debug.Log(">> ДПС низкий, но потерь нет (Лабиринт/Контроль). Сложность не меняем.");
			}
		}

		PerformanceMonitor.instance.ResetWaveStats();
	}
}