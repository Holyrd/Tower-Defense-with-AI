using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D rb;

	[Header("Attribute")]
	[SerializeField] private float moveSpeed = 2f; // Увеличил, 0.2 очень мало

	private List<Node> path; // ТЕПЕРЬ ПУТЬ ЭТО СПИСОК УЗЛОВ
	private int targetIndex = 0;
	private float baseSpeed;

	// Нам нужна цель (Конец карты). Допустим, она есть в LevelManager
	private Vector3 endPoint;

	private void Start()
	{
		baseSpeed = moveSpeed;

		// Получаем точку конца (последняя точка старого пути)
		// ВНИМАНИЕ: Убедись, что в LevelManager.main.path есть хотя бы одна точка
		if (LevelManager.main.path.Length > 0)
			endPoint = LevelManager.main.path[LevelManager.main.path.Length - 1].position;
		else
			endPoint = Vector3.zero; // Заглушка

		// ЗАПРАШИВАЕМ ПУТЬ
		GetCachedPath();
	}

	public void GetCachedPath()
	{
		// Проверяем, есть ли рассчитанный путь
		if (Pathfinder.main.currentWavePath != null && Pathfinder.main.currentWavePath.Count > 0)
		{
			// ВАЖНО: Создаем новый список на основе старого (копию),
			// чтобы враги не мешали друг другу
			path = new List<Node>(Pathfinder.main.currentWavePath);
			targetIndex = 0;
		}
	}

	private void Update()
	{
		if (path == null) return;

		// Смотрим на текущую точку пути
		Vector3 currentWaypoint = path[targetIndex].worldPosition;

		if (Vector2.Distance(transform.position, currentWaypoint) <= 0.1f)
		{
			targetIndex++;
			if (targetIndex >= path.Count)
			{
				// ДОШЛИ ДО КОНЦА
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
