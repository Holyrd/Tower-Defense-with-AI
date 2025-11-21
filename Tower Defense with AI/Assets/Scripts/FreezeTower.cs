using System.Collections;
using UnityEditor;
using UnityEngine;

public class FreezeTower : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private LayerMask enemyMask;

	[Header("Properties")]
	[SerializeField] private float targetingRange = 3f;
	[SerializeField] private float aps = 0.25f; //attack per seconds
	[SerializeField] private float enemySpeedAfterFreeze = 0.5f;
	[SerializeField] private float freezeTime = 1f;

	private float timeUntilFire;

	private void Update()
	{
		timeUntilFire += Time.deltaTime;

		if (timeUntilFire >= 1f / aps)
		{
			FreezeEnemies();
			timeUntilFire = 0f;
		}
	}

	private void FreezeEnemies()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

		if (hits.Length > 0)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				RaycastHit2D hit = hits[i];

				EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
				em.ChangeSpeed(enemySpeedAfterFreeze);
				StartCoroutine(ResetAfterFreeze(em));
			}
		}
	}

	private IEnumerator ResetAfterFreeze(EnemyMovement em)
	{
		yield return new WaitForSeconds(freezeTime);

		em.ResetSpeed();
	}

	private void OnDrawGizmosSelected()
	{
		Handles.color = Color.cyan;
		Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
	}
}
