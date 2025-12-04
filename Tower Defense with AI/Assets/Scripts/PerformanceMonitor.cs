using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
	public static PerformanceMonitor instance;

	public float TotalDamageDealt { get; private set; }
	public int HealthLostInCurrentWave { get; private set; }

	private float waveStartTime = -1f;
	private float waveEndTime = -1f;
	private bool isWaveActive = false;
	private float spawnDuration;

	private void Awake() { instance = this; }

	public void StartWaveMonitoring()
	{
		ResetWaveStats();
		waveStartTime = Time.time;
		isWaveActive = true;
	}

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

	public void RegisterDamage(float amount)
	{
		if (isWaveActive)
		{
			TotalDamageDealt += amount;
		}
	}

	public void RegisterHealthLoss(int damage)
	{
		HealthLostInCurrentWave += damage;
	}

	public void SetSpawnDuration(float _spawnDuration)
	{
		spawnDuration = _spawnDuration;
	}

	public float GetRealDPS()
	{
		float endTime = isWaveActive ? Time.time : waveEndTime;

		float duration = endTime - waveStartTime;

		if (duration < 1f) duration = 1f;

		return TotalDamageDealt / duration;
	}
}