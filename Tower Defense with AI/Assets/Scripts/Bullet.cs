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
		collision.gameObject.GetComponent<Health>().TakeDamage(damage);
		Destroy(gameObject);
	}
}
