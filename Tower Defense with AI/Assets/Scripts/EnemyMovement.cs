using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D rb;

	[Header("Attribute")]
	[SerializeField] private float moveSpeed = 0.2f;

	private Transform target;
	private int targetIndex = 0;

	private float baseSpeed;

	private void Start()
	{
		baseSpeed = moveSpeed;
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
				EnemySpawner.OnEnemyDestroy.Invoke();
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

	public void ChangeSpeed(float updatedSpeed)
	{
		moveSpeed = updatedSpeed;
	}

	public void ResetSpeed()
	{
		moveSpeed = baseSpeed;
	}
}
