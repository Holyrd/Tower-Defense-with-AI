using UnityEngine;

public class Bullet : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float speed = 10f;
	[SerializeField] private GameObject explosionEffect;

	private Transform target;
	private int damage;
	private float explosionRadius;
	private float liveTime;

	// НОВОЕ: Зберігаємо напрямок руху
	private Vector3 moveDirection;

	public void SetTarget(Transform _target)
	{
		target = _target;
		// Одразу розраховуємо напрямок при старті, щоб знати куди летіти
		UpdateDirection();
	}

	public void SetDamage(int _damage)
	{
		damage = _damage;
	}

	public void SetExplosionRadius(float _radius)
	{
		explosionRadius = _radius;
	}

	private void Update()
	{
		liveTime += Time.deltaTime;
		if (liveTime >= 10f)
		{
			Destroy(gameObject);
		}
		// 1. Оновлюємо напрямок, ТІЛЬКИ якщо ціль жива
		if (target != null)
		{
			UpdateDirection();
		}
		// Якщо target == null, moveDirection залишається старим (куля летить далі)

		// 2. Рух
		float distanceThisFrame = speed * Time.deltaTime;
		transform.Translate(moveDirection * distanceThisFrame, Space.World);

		// 3. Поворот
		float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	// Допоміжний метод для розрахунку вектора
	private void UpdateDirection()
	{
		moveDirection = (target.position - transform.position).normalized;
	}

	// НОВОЕ: Використовуємо Unity фізику для виявлення влучань
	private void OnCollisionEnter2D(Collision2D target)
	{
		// Перевіряємо, чи врізалися ми у ворога
		if (target.gameObject.CompareTag("Enemy"))
		{
			HitTarget(target.gameObject); // Б'ємо того, в кого врізалися
		}
	}

	// Змінили метод, тепер він приймає transform того, в кого влучили
	private void HitTarget(GameObject hitEnemy)
	{
		if (explosionRadius > 0f)
		{
			Explode(); // Вибух (логіка не змінюється, б'є по площі)
		}
		else
		{
			DamageSingle(hitEnemy); // Б'ємо конкретну ціль
		}

		if (explosionEffect != null)
		{
			Instantiate(explosionEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}

	private void DamageSingle(GameObject _target)
	{
		Health e = _target.GetComponent<Health>();
		if (e != null)
		{
			PerformanceMonitor.instance.RegisterDamage(damage);
			e.TakeDamage(damage);
		}
	}

	private void Explode()
	{
		int enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayerMask);

		int count = 0;
		foreach (Collider2D collider in colliders)
		{
			if (collider.CompareTag("Enemy") && count <= 5)
			{
				count++;
				DamageSingle(collider.gameObject);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}