using UnityEngine;
using System.Collections.Generic;

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

	// Генерация полного списка врагов для волны с учетом множителя сложности
	public List<GameObject> GetGeneratedWaveList(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		List<GameObject> finalEnemies = new List<GameObject>();

		// 1. Наполняем список (как и раньше)
		foreach (var group in data.enemyGroups)
		{
			// Применяем множитель сложности к количеству
			int adjustedCount = Mathf.RoundToInt(group.count * currentCountMult);

			for (int i = 0; i < adjustedCount; i++)
			{
				finalEnemies.Add(group.enemyPrefab);
			}
		}

		// 2. ПЕРЕМЕШИВАЕМ СПИСОК (Fisher-Yates Shuffle)
		// Проходим по списку и меняем каждый элемент со случайным другим
		for (int i = 0; i < finalEnemies.Count; i++)
		{
			GameObject temp = finalEnemies[i];
			int randomIndex = Random.Range(i, finalEnemies.Count);

			finalEnemies[i] = finalEnemies[randomIndex];
			finalEnemies[randomIndex] = temp;
		}

		return finalEnemies;
	}

	public float GetAdjustedSpawnRate(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		return data.spawnRate * currentRateMult * currentProfile.baseSpeedMultiplier;
	}

	// --- ГЛАВНЫЙ МОЗГ (МАТЕМАТИКА ХП) ---

	// Подсчет общего ХП волны (Предсказание)
	private float CalculateTotalWaveHP(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		float totalHP = 0;

		foreach (var group in data.enemyGroups)
		{
			if (group.enemyPrefab == null) continue;

			// Получаем ХП одного врага (через GetComponent или Health.maxHitPoint)
			float singleHP = 0;
			var healthScript = group.enemyPrefab.GetComponent<Health>();
			if (healthScript != null)
				singleHP = healthScript.GetMaxHealth(); // Убедись, что метод есть в Health!

			// Учитываем множитель количества врагов
			int count = Mathf.RoundToInt(group.count * currentCountMult);
			totalHP += singleHP * count;
		}
		return totalHP;
	}

	public void EvaluateAndAdjust(int waveJustFinished)
	{
		// 1. Считаем, сколько ХП было в прошлой волне
		float waveTotalHP = CalculateTotalWaveHP(waveJustFinished);

		// 2. Узнаем скорость спавна той волны (сколько длилась волна в идеале)
		float spawnRate = GetAdjustedSpawnRate(waveJustFinished);
		float enemyCount = GetGeneratedWaveList(waveJustFinished).Count;

		// Время спавна всей волны (секунды) = Кол-во / Скорость
		float waveDuration = enemyCount / Mathf.Max(spawnRate, 0.1f);

		// 3. Считаем ТРЕБУЕМЫЙ DPS (Required DPS)
		// Чтобы убить всех ровно за время их выхода:
		float requiredDPS = waveTotalHP / Mathf.Max(waveDuration, 1f);

		// 4. Узнаем РЕАЛЬНЫЙ DPS игрока (Active Combat DPS)
		float playerDPS = PerformanceMonitor.instance.GetAverageDPS();

		// Защита от нулей
		if (playerDPS < 1) playerDPS = 1;

		// 5. Сравниваем (Ratio)
		// Если Required = 100, а Player = 150 -> Ratio = 1.5 (Игрок на 50% сильнее нормы)
		float performanceRatio = playerDPS / requiredDPS;

		// Коррекция на профиль сложности (на Харде мы требуем большего)
		performanceRatio /= currentProfile.scoreRequirementMultiplier;

		Debug.Log($"АНАЛИЗ ВОЛНЫ: TotalHP: {waveTotalHP}, Duration: {waveDuration:F1}s. " +
				  $"Нужен DPS: {requiredDPS:F1}, У игрока: {playerDPS:F1}. Ratio: {performanceRatio:F2}");

		// 6. Вердикт
		if (performanceRatio > rules.highPerformanceThreshold)
		{
			currentCountMult *= rules.enemyCountMultiplier_Harder;
			currentRateMult *= rules.spawnRateMultiplier_Harder;
			currentGoldMult *= rules.goldRewardMultiplier_Bonus;
			Debug.Log(">> СЛИШКОМ ЛЕГКО -> УСЛОЖНЯЕМ!");
		}
		else if (performanceRatio < rules.lowPerformanceThreshold)
		{
			currentCountMult *= rules.enemyCountMultiplier_Easier;
			currentRateMult *= rules.spawnRateMultiplier_Easier;
			currentGoldMult = 1.0f;
			Debug.Log(">> СЛИШКОМ ТЯЖЕЛО -> ОБЛЕГЧАЕМ.");
		}

		PerformanceMonitor.instance.ResetWaveStats();
	}
}