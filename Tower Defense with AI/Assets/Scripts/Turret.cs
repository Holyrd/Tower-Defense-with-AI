using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform rotationPoint;
	[SerializeField] private LayerMask enemyMask;
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform firePoint;
	[SerializeField] private GameObject upgradeUI;
	[SerializeField] private Button upgradeButton;
	[SerializeField] TextMeshProUGUI upgradeCostUI;

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer[] turretSprites;
	[SerializeField] private Sprite[] newTurretSprites;

	[Header("Properties")]
	[SerializeField] private float targetingRange = 3f;
	[SerializeField] private float rotationSpeed = 200f;
	[SerializeField] private float bps = 1f; // Bullet per seconds
	[SerializeField] private int upgradeCost = 100;


	private Transform target;
	private float timeUntilFire;


	private void Start()
	{
		upgradeButton.onClick.AddListener(Upgrade);
	}

	private void Update()
	{
		if (target == null)
		{
			FindTarget();
			return;
		}

		RotateTowardsTarget();

		if (!CheckTargetIsInRange())
		{
			target = null;
		}
		else
		{

			timeUntilFire += Time.deltaTime;

			if (timeUntilFire >= 1f / bps)
			{
				Shoot();
				timeUntilFire = 0f;
			}
		}
	}

	private void Shoot()
	{
		GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
		Bullet bulletScript = bulletObj.GetComponent<Bullet>();
		bulletScript.SetTarget(target);
	}

	private void FindTarget()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

		if (hits.Length > 0)
		{
			target = hits[0].transform;
		}
	}

	private bool CheckTargetIsInRange()
	{
		return Vector2.Distance(target.position, transform.position) <= targetingRange;
	}

	private void RotateTowardsTarget()
	{
		float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg + 90f;

		Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
		rotationPoint.rotation = Quaternion.RotateTowards(rotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
	}

	public void OpenUpgradeUI()
	{
		upgradeUI.SetActive(true);
		upgradeCostUI.text = upgradeCost.ToString();
	}

	public void CloseUpgradeUI()
	{
		upgradeUI.SetActive(false);
		UIManager.main.SetHoveringState(false);
	}

	private void Upgrade()
	{
		if (upgradeCost > LevelManager.main.currency) return;

		LevelManager.main.SpentCurrency(upgradeCost);

		for (int i = 0; i < turretSprites.Length; i++)
		{
			if (turretSprites[i] != null && newTurretSprites[i] != null)
			{
				turretSprites[i].sprite = newTurretSprites[i];
			}
		}

		CloseUpgradeUI();
		Debug.Log("I get upgrade!");
	}


	private void OnDrawGizmosSelected()
	{
		Handles.color = Color.cyan;
		Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
	}
}
