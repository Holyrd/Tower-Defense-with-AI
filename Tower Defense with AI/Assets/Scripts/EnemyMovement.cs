using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D rb;

	[Header("Attribute")]
	[SerializeField] private float moveSpeed = 2f; 

	private List<Node> path; 
	private int targetIndex = 0;
	private float baseSpeed;

	private Vector3 endPoint;

	private void Start()
	{
		baseSpeed = moveSpeed;

		if (Pathfinder.main.startPoint != null)
			this.transform.position = Pathfinder.main.startPoint.position;

		GetCachedPath();
	}

	public void GetCachedPath()
	{
		if (Pathfinder.main.currentWavePath != null && Pathfinder.main.currentWavePath.Count > 0)
		{
			path = new List<Node>(Pathfinder.main.currentWavePath);
			targetIndex = 0;
		}
	}

	private void Update()
	{
		if (path == null) return;

		Vector3 currentWaypoint = path[targetIndex].worldPosition;

		if (Vector2.Distance(transform.position, currentWaypoint) <= 0.1f)
		{
			targetIndex++;
			if (targetIndex >= path.Count)
			{
				EnemySpawner.OnEnemyDestroy.Invoke();
				LevelManager.main.GetDamage(this.GetComponent<Health>().GiveDamage());
				Destroy(gameObject);
				return;
			}
		}
	}

	private void FixedUpdate()
	{
		if (path == null || targetIndex >= path.Count) return;

		Vector3 currentWaypoint = path[targetIndex].worldPosition;
		Vector2 direction = (currentWaypoint - transform.position).normalized;
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
