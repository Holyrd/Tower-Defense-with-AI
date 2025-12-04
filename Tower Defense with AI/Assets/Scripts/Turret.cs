using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TurretUpgradeStep
{
	[Header("Cost to upgrade to this level")]
	public int price;

	[Header("Increases to characteristics")]
	public int damageBonus;      
	public float rangeBonus;     
	public float bpsBonus;       
	public float explosionRadiusBonus;

	[Header("Visuals")]
	public Sprite[] newTurretSprites;
}

public class Turret : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform rotationPoint;
	[SerializeField] private LayerMask enemyMask;
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private Transform firePoint;

	[Header("UI References")]
	[SerializeField] private GameObject upgradeUI;
	[SerializeField] private Button upgradeButton;
	[SerializeField] TextMeshProUGUI upgradeCostText; 
	[SerializeField] TextMeshProUGUI upgradeInfoText; 

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer[] turretSprites;

	[Header("Base Stats (Start Level)")]
	[SerializeField] private float targetingRange = 3f;
	[SerializeField] private float rotationSpeed = 200f;
	[SerializeField] private float bps = 1f;
	[SerializeField] private int baseDamage = 10; 
	[SerializeField] private float baseExplosionRadius = 0f;

	[Header("Upgrade Settings")]
	public TurretUpgradeStep[] upgrades;

	private Transform target;
	private float timeUntilFire;
	private int currentLevel = 0; 
	private int currentDamage;    
	private float currentExplosionRadius;

	private void Start()
	{
		currentDamage = baseDamage; 
		currentExplosionRadius = baseExplosionRadius;
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

		if (bulletScript != null)
		{
			bulletScript.SetTarget(target);
			bulletScript.SetDamage(currentDamage);

			bulletScript.SetExplosionRadius(currentExplosionRadius);
		}
	}

	private void FindTarget()
	{
		RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);
		if (hits.Length > 0)
		{
			target = hits[hits.Length - 1].transform;
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

	public float GetPotentialDPS()
	{
		if (currentExplosionRadius > 0)
		{
			return currentDamage * 5 * bps;
		}
		else
		{
			return currentDamage * bps;
		}
	}

	private void OnMouseDown()
	{
		if (UIManager.main.IsHoveringUI()) return;
		OpenUpgradeUI();
	}

	public void OpenUpgradeUI()
	{
		upgradeUI.SetActive(true);
		UpdateUI();
	}

	public void CloseUpgradeUI()
	{
		upgradeUI.SetActive(false);
		UIManager.main.SetHoveringState(false);
	}

	private void UpdateUI()
	{
		if (currentLevel >= upgrades.Length)
		{
			upgradeCostText.text = "MAX";
			upgradeButton.interactable = false; 
		}
		else
		{
			TurretUpgradeStep nextUpgrade = upgrades[currentLevel];
			upgradeCostText.text = nextUpgrade.price.ToString();
			upgradeButton.interactable = true;

			if (upgradeInfoText)
				upgradeInfoText.text = $"+{nextUpgrade.damageBonus} Dmg\n+{nextUpgrade.bpsBonus} Spd";
		}
	}

	private void Upgrade()
	{
		if (currentLevel >= upgrades.Length) return;

		TurretUpgradeStep step = upgrades[currentLevel];

		if (LevelManager.main.currency < step.price)
		{
			UIManager.main.SetHoveringState(false);
			Debug.Log("Not enough money!");
			return;
		}

		LevelManager.main.SpentCurrency(step.price);
		StatsManager.main.TrackMoneySpent(step.price); 

		currentDamage += step.damageBonus;
		targetingRange += step.rangeBonus;
		bps += step.bpsBonus;
		currentExplosionRadius += step.explosionRadiusBonus;

		currentLevel++;

		if (step.newTurretSprites != null && step.newTurretSprites.Length > 0)
		{
			for (int i = 0; i < turretSprites.Length; i++)
			{
				if (i < step.newTurretSprites.Length && step.newTurretSprites[i] != null)
				{
					turretSprites[i].sprite = step.newTurretSprites[i];
				}
			}
		}

		Debug.Log($"Tower upgraded to level {currentLevel}. Damage: {currentDamage}, BPS: {bps}");

		
		UpdateUI();
	}
}