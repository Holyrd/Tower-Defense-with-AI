using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
	public static PerformanceMonitor instance;

	// --- Статистика за текущую волну ---
	public float TotalDamageDealt { get; private set; }
	public int HealthLostInCurrentWave { get; private set; }

	// Тайминги волны
	private float waveStartTime = -1f;
	private float waveEndTime = -1f;
	private bool isWaveActive = false;
	private float spawnDuration;

	private void Awake() { instance = this; }

	// Вызывается Спавнером, когда рождается первый враг
	public void StartWaveMonitoring()
	{
		ResetWaveStats();
		waveStartTime = Time.time;
		isWaveActive = true;
	}

	// Вызывается Спавнером, когда все мертвы
	public void StopWaveMonitoring()
	{
		if (isWaveActive)
		{
			waveEndTime = Time.time;
			isWaveActive = false;
		}
	}

	public void ResetWaveStats()
	{
		TotalDamageDealt = 0;
		HealthLostInCurrentWave = 0;
		waveStartTime = -1f;
		waveEndTime = -1f;
		isWaveActive = false;
		spawnDuration = 0;
	}

	// --- Сбор данных ---

	// Вызывается из Health.cs (при попадании)
	public void RegisterDamage(float amount)
	{
		if (isWaveActive)
		{
			TotalDamageDealt += amount;
		}
	}

	// Вызывается из LevelManager (при пропуске врага)
	public void RegisterHealthLoss(int damage)
	{
		HealthLostInCurrentWave += damage;
	}

	public void SetSpawnDuration(float _spawnDuration)
	{
		spawnDuration = _spawnDuration;
	}

	// --- Расчет Реального ДПС ---
	public float GetRealDPS()
	{
		float endTime = isWaveActive ? Time.time : waveEndTime;

		// Длительность волны
		float duration = endTime - waveStartTime + spawnDuration;

		// Защита от деления на ноль (если волна длилась 0 сек)
		if (duration < 1f) duration = 1f;

		// Твоя формула: Урон за волну / Время волны
		return TotalDamageDealt / duration;
	}
}