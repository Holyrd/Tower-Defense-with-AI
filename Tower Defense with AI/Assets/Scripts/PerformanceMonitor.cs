using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
	public static PerformanceMonitor instance;

	// Статистика за текущую волну
	public float TotalDamageDealt { get; private set; }

	// Чистое время, когда башни реально стреляли и наносили урон
	private float activeCombatTime = 0f;

	// Время последнего полученного урона
	private float lastDamageTime = -100f;

	// Окно активности: если урона не было дольше 1 сек, считаем что бой прекратился
	private const float COMBAT_TIMEOUT = 1.0f;

	private void Awake() { instance = this; }

	private void Update()
	{
		// Если с момента последнего удара прошло меньше секунды -> считаем это время боевым
		if (Time.time - lastDamageTime < COMBAT_TIMEOUT)
		{
			activeCombatTime += Time.deltaTime;
		}
	}

	public void ResetWaveStats()
	{
		TotalDamageDealt = 0;
		activeCombatTime = 0;
		lastDamageTime = -100f;
	}

	public void RegisterDamage(float amount)
	{
		// Обновляем метку времени "Мы в бою!"
		lastDamageTime = Time.time;
		TotalDamageDealt += amount;
	}

	// Получить "Честный DPS" (Урон / Время активной стрельбы)
	public float GetAverageDPS()
	{
		// Защита: если бой длился 0.1 сек, округлим до 1, чтобы не делить на ноль
		// и не получать миллиардные значения DPS от одного выстрела
		float time = Mathf.Max(activeCombatTime, 1f);

		return TotalDamageDealt / time;
	}
}