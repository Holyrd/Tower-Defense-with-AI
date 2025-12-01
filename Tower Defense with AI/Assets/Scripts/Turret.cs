using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TurretUpgradeStep
{
	[Header("Стоимость улучшения до этого уровня")]
	public int price;

	[Header("Прибавки к характеристикам")]
	public int damageBonus;      // + урон
	public float rangeBonus;     // + радиус
	public float bpsBonus;       // + скорострельность
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
	[SerializeField] TextMeshProUGUI upgradeCostText; // Переименовал для ясности
	[SerializeField] TextMeshProUGUI upgradeInfoText; // (Опционально) Текст, что даст апгрейд

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer[] turretSprites;

	[Header("Base Stats (Start Level)")]
	[SerializeField] private float targetingRange = 3f;
	[SerializeField] private float rotationSpeed = 200f;
	[SerializeField] private float bps = 1f;
	[SerializeField] private int baseDamage = 10; // НОВОЕ: Базовый урон башни
	[SerializeField] private float baseExplosionRadius = 0f;

	[Header("Upgrade Settings")]
	// Массив улучшений. Размер массива = количество возможных улучшений.
	public TurretUpgradeStep[] upgrades;

	// Внутренние переменные
	private Transform target;
	private float timeUntilFire;
	private int currentLevel = 0; // Текущий уровень (0 = база)
	private int currentDamage;    // Текущий урон (база + бонусы)
	private float currentExplosionRadius;

	private void Start()
	{
		currentDamage = baseDamage; // На старте урон равен базовому
		currentExplosionRadius = baseExplosionRadius;
		upgradeButton.onClick.AddListener(Upgrade);
	}

	private void Update()
	{
		// Логика стрельбы и поворота
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

			// --- НОВОЕ: Передаем пуле радиус взрыва ---
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

	// --- НОВОЕ: Метод для оценки силы башни (DDA) ---
	public float GetPotentialDPS()
	{
		// DPS = Урон * Скорострельность
		// Если у тебя есть лазер, формула может отличаться, но для пуль так:
		if (currentExplosionRadius > 0)
		{
			return currentDamage * 5 * bps;
		}
		else
		{
			return currentDamage * bps;
		}
	}

	// --- ЛОГИКА UI И АПГРЕЙДОВ ---

	private void OnMouseDown()
	{
		if (UIManager.main.IsHoveringUI()) return;
		OpenUpgradeUI();
	}

	public void OpenUpgradeUI()
	{
		upgradeUI.SetActive(true);
		UpdateUI(); // Обновляем текст на кнопке
	}

	public void CloseUpgradeUI()
	{
		upgradeUI.SetActive(false);
		UIManager.main.SetHoveringState(false);
	}

	private void UpdateUI()
	{
		// Если мы достигли макс уровня
		if (currentLevel >= upgrades.Length)
		{
			upgradeCostText.text = "MAX";
			upgradeButton.interactable = false; // Выключаем кнопку
		}
		else
		{
			// Берем данные следующего уровня
			TurretUpgradeStep nextUpgrade = upgrades[currentLevel];
			upgradeCostText.text = nextUpgrade.price.ToString();
			upgradeButton.interactable = true;

			// Если есть текстовое поле для инфо, можно вывести: "+5 DMG"
			if (upgradeInfoText)
				upgradeInfoText.text = $"+{nextUpgrade.damageBonus} Dmg\n+{nextUpgrade.bpsBonus} Spd";
		}
	}

	private void Upgrade()
	{
		// 1. Проверка на макс уровень
		if (currentLevel >= upgrades.Length) return;

		// 2. Берем данные текущего шага
		TurretUpgradeStep step = upgrades[currentLevel];

		// 3. Проверка денег
		if (LevelManager.main.currency < step.price)
		{
			UIManager.main.SetHoveringState(false);
			Debug.Log("Not enough money!");
			return;
		}

		// 4. Тратим деньги
		LevelManager.main.SpentCurrency(step.price);
		StatsManager.main.TrackMoneySpent(step.price); // Если используешь статистику

		// 5. Применяем улучшения (Прибавка к текущим)
		currentDamage += step.damageBonus;
		targetingRange += step.rangeBonus;
		bps += step.bpsBonus;
		currentExplosionRadius += step.explosionRadiusBonus;

		// 6. Повышаем уровень
		currentLevel++;

		if (step.newTurretSprites != null && step.newTurretSprites.Length > 0)
		{
			// Идем по списку рендереров башни
			for (int i = 0; i < turretSprites.Length; i++)
			{
				// Если для этого рендерера есть новый спрайт в списке улучшения
				if (i < step.newTurretSprites.Length && step.newTurretSprites[i] != null)
				{
					turretSprites[i].sprite = step.newTurretSprites[i];
				}
			}
		}

		Debug.Log($"Башня улучшена до уровня {currentLevel}. Урон: {currentDamage}, Скорость: {bps}");

		// 7. Обновляем UI (чтобы показалась цена следующего уровня или MAX)
		UpdateUI();


		//CloseUpgradeUI(); 
	}
}