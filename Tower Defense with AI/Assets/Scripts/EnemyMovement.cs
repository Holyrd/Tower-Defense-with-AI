using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D rb;

	[Header("Attribute")]
	[SerializeField] private float moveSpeed = 0.2f;

	private Transform target;
	private int targetIndex = 0;


	private void Start()
	{
		this.transform.position = LevelManager.main.startPoint.position;
		target = LevelManager.main.path[targetIndex];

	}

	private void Update()
	{
		if (Vector2.Distance(target.position, transform.position) <= 0.1f)
		{
			targetIndex++;
			if (targetIndex == LevelManager.main.path.Length)
			{
				Destroy(gameObject);
				return;
			}
			else
			{
				target = LevelManager.main.path[targetIndex];
			}
		}
	}

	private void FixedUpdate()
	{
		Vector2 direction = (target.position - transform.position).normalized;

		rb.linearVelocity = direction * moveSpeed;
	}
}
