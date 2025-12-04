using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicDifficultyManager : MonoBehaviour
{
	public static DynamicDifficultyManager instance;

	[Header("Config file")]
	public WaveConfigSO baseWaves;
	public AdjustmentRulesSO rules;

	[Header("Difficulty rule")]
	public AdjustmentRulesSO rulesEasy;   
	public AdjustmentRulesSO rulesNormal; 
	public AdjustmentRulesSO rulesHard;   

	[Header("Balance")]
	public float currentCountMult = 1.0f;
	public float currentRateMult = 1.0f;
	public float currentGoldMult = 1.0f;

	public enum DDAState { Normal, Hard, Easy }
	public event Action<DDAState> OnDifficultyChanged;

	private void Awake()
	{
		instance = this;
		LoadDifficultySettings();
	}

	private void LoadDifficultySettings()
	{
		// Получаем сохраненное значение (0=Easy, 1=Normal, 2=Hard). По умолчанию 1 (Normal).
		int difficultyIndex = PlayerPrefs.GetInt("SelectedDifficulty", 1);

		switch (difficultyIndex)
		{
			case 0:
				rules = rulesEasy;
				Debug.Log("DDA: Загружены правила ЛЕГКОЙ сложности.");
				break;
			case 2:
				rules = rulesHard;
				Debug.Log("DDA: Загружены правила ТЯЖЕЛОЙ сложности.");
				break;
			case 1:
			default:
				rules = rulesNormal;
				Debug.Log("DDA: Загружены правила НОРМАЛЬНОЙ сложности.");
				break;
		}

		// Защита от дурака: если забыл перетянуть файл в инспекторе
		if (rules == null)
		{
			Debug.LogError("ОШИБКА! Не назначены AdjustmentRules в инспекторе! Использую дефолт.");
			rules = ScriptableObject.CreateInstance<AdjustmentRulesSO>();
		}
	}


	private WaveConfigSO.WaveData GetSafeWaveData(int waveIndex)
	{
		if (baseWaves.waves.Count == 0) return new WaveConfigSO.WaveData();
		int safeIndex = Mathf.Clamp(waveIndex, 0, baseWaves.waves.Count - 1);
		return baseWaves.waves[safeIndex];
	}

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
		return GetSafeWaveData(waveIndex).spawnRate * currentRateMult;
	}

	public List<GameObject> GetGeneratedWaveList(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		List<GameObject> finalEnemies = new List<GameObject>();

		if (data.enemyGroups != null)
		{
			foreach (var group in data.enemyGroups)
			{
				int adjustedCount = Mathf.RoundToInt(group.count * currentCountMult);

				for (int i = 0; i < adjustedCount; i++)
				{
					finalEnemies.Add(group.enemyPrefab);
				}
			}
		}

		for (int i = 0; i < finalEnemies.Count; i++)
		{
			GameObject temp = finalEnemies[i];
			int randomIndex = UnityEngine.Random.Range(i, finalEnemies.Count);

			finalEnemies[i] = finalEnemies[randomIndex];
			finalEnemies[randomIndex] = temp;
		}

		return finalEnemies;
	}

	private float CalculateTotalWaveHP(int waveIndex)
	{
		var data = GetSafeWaveData(waveIndex);
		float totalHP = 0;

		if (data.enemyGroups != null)
		{
			foreach (var group in data.enemyGroups)
			{
				if (group.enemyPrefab == null) continue;

				float singleEnemyHP = 0;
				var hpScript = group.enemyPrefab.GetComponent<Health>();

				if (hpScript) singleEnemyHP = hpScript.GetMaxHealth();

				int adjustedCount = Mathf.RoundToInt(group.count * currentCountMult);

				totalHP += singleEnemyHP * adjustedCount;
			}
		}

		return totalHP;
	}

	private float CalculatePlayerPotentialDPS()
	{
		float totalPotential = 0f;

		Turret[] turrets = FindObjectsByType<Turret>(FindObjectsSortMode.None);
		foreach (var t in turrets)
		{
			totalPotential += t.GetPotentialDPS();
		}
		return totalPotential;
	}

	public DDAState GetCurrentState()
	{
		if (currentCountMult > 1.05f) return DDAState.Hard;
		if (currentCountMult < 0.95f) return DDAState.Easy;

		return DDAState.Normal;
	}

	public void EvaluateAndAdjust(int waveJustFinished)
	{
		float waveHP = CalculateTotalWaveHP(waveJustFinished);
		float spawnRate = GetAdjustedSpawnRate(waveJustFinished);
		int count = GetAdjustedEnemyCount(waveJustFinished);

		float spawnDuration = count / Mathf.Max(spawnRate, 0.1f) - 2f;

		float requiredDPS = waveHP / Mathf.Max(spawnDuration, 0.1f);

		PerformanceMonitor.instance.SetSpawnDuration(spawnDuration);
		float realDPS = PerformanceMonitor.instance.GetRealDPS();
		int healthLost = PerformanceMonitor.instance.HealthLostInCurrentWave;

		float potentialDPS = CalculatePlayerPotentialDPS();

		Debug.Log($"DDA Report: ReqDPS={requiredDPS:F1}, RealDPS={realDPS:F1}, Potential={potentialDPS:F1}, HP Lost={healthLost}");

		if (potentialDPS > requiredDPS * rules.highPerformanceThreshold && waveJustFinished > 4)
		{
			if (realDPS >= potentialDPS * 0.6f)
			{
				currentCountMult *= rules.enemyCountMultiplier_Harder;
				currentRateMult *= rules.spawnRateMultiplier_Harder;
				currentGoldMult *= rules.goldRewardMultiplier_Bonus;
				Debug.Log(">> Try to improve difficult");
			}
			else if(realDPS <= potentialDPS * 0.6f && healthLost == 0)
			{
				currentCountMult *= rules.enemyCountMultiplier_Harder;
				currentRateMult *= rules.spawnRateMultiplier_Harder;
				currentGoldMult *= rules.goldRewardMultiplier_Bonus;
				Debug.Log(">> Try to improve difficult");
			}

		}

		else if (realDPS < requiredDPS * rules.lowPerformanceThreshold)
		{
			if (healthLost > 0)
			{
				currentCountMult *= rules.enemyCountMultiplier_Easier;
				currentRateMult *= rules.spawnRateMultiplier_Easier;
				currentGoldMult = 1.0f;
				Debug.Log(">> Try to decries difficult");
			}
			else
			{
				Debug.Log(">> Low DPS. But not HP lost");
			}
		}

		currentCountMult = Mathf.Clamp(currentCountMult, rules.minDifficultyLimit, rules.maxDifficultyLimit);
		currentRateMult = Mathf.Clamp(currentRateMult, rules.minDifficultyLimit, rules.maxDifficultyLimit);

		currentGoldMult = Mathf.Clamp(currentGoldMult, 1.0f, 1.2f);

		DDAState newState = GetCurrentState();
		OnDifficultyChanged?.Invoke(newState);

		PerformanceMonitor.instance.ResetWaveStats();
	}
}