using UnityEngine;

public class Bullet : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D rb;

	[Header("Attribute")]
	[SerializeField] private float bulletSpeed = 5f;
	[SerializeField] private int damage = 1;

	private Transform target;
	private float liveTime = 5f;
	private float currentLiveTime;
	private bool _hasHit = false;

	public int GetDamage()
	{
		return damage;
	}

	public void SetTarget(Transform _target)
	{
		target = _target;
	}

	private void FixedUpdate()
	{
		if(!target)
		{
			return;
		}
		Vector2 direction = (target.position - transform.position).normalized;

		rb.linearVelocity = direction * bulletSpeed;
	}

	private void Update()
	{
		currentLiveTime += Time.deltaTime;
		if(currentLiveTime >= liveTime)
		{
			Destroy(gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (_hasHit) return;

		// Сразу ставим флаг, блокируя последующие вызовы
		_hasHit = true;

		// --- НОВАЯ ЛОГИКА: СООБЩАЕМ О ВЫСТРЕЛЕ ---
		if (PerformanceMonitor.instance != null)
		{
			// Передаем урон пули в статистику
			PerformanceMonitor.instance.RegisterDamage(GetDamage());
		}

		// Хорошей практикой считается проверка на null (на случай если у объекта нет скрипта Health)
		if (collision.gameObject.TryGetComponent<Health>(out Health health))
		{
			health.TakeDamage(damage);
		}

		Destroy(gameObject);
	}
}
