using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class FarmUpgradeStep
{
	[Header("Стоимость улучшения")]
	public int price;

	[Header("Бонусы")]
	public int incomeBonus;        // + к добыче за цикл
	public float speedBonus;       // Уменьшение времени цикла (например, 0.5 = на 0.5 сек быстрее)

	[Header("Визуал")]
	public Sprite[] newFarmSprites; // Новые спрайты здания
}

public class Farm : MonoBehaviour
{
	[Header("References")]
	// Ссылка на эффект (например, монетка всплывает), если есть
	[SerializeField] private GameObject incomeEffect;
	[SerializeField] private Transform incomePoint; // Точка, где появляется эффект

	[Header("UI References")]
	[SerializeField] private GameObject upgradeUI;
	[SerializeField] private Button upgradeButton;
	[SerializeField] TextMeshProUGUI upgradeCostText;
	[SerializeField] TextMeshProUGUI upgradeInfoText;

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer[] farmSprites; // Основа и, например, анимация кирки/вентилятора

	[Header("Base Stats")]
	[SerializeField] private int baseIncome = 15;      // Сколько дает денег
	[SerializeField] private float baseCycleTime = 5f; // Как часто (раз в 5 секунд)

	[Header("Upgrades")]
	public FarmUpgradeStep[] upgrades;

	// Внутренние переменные
	private float timer;
	private int currentIncome;
	private float currentCycleTime;
	private int currentLevel = 0;

	private void Start()
	{
		// Инициализация статов
		currentIncome = baseIncome;
		currentCycleTime = baseCycleTime;
		timer = currentCycleTime;

		upgradeButton.onClick.AddListener(Upgrade);
		UpdateUI();
	}

	private void Update()
	{
		// Таймер добычи
		timer -= Time.deltaTime;

		if (timer <= 0f)
		{
			GenerateMoney();
			timer = currentCycleTime;
		}
	}

	private void GenerateMoney()
	{
		// 1. Даем деньги игроку
		LevelManager.main.IncreasCurrency(currentIncome);

		// 2. Пишем в статистику
		if (StatsManager.main != null)
			StatsManager.main.TrackMoneyEarned(currentIncome);

		// 3. Визуальный эффект (опционально)
		if (incomeEffect != null && incomePoint != null)
		{
			Instantiate(incomeEffect, incomePoint.position, Quaternion.identity);
		}

		// Можно добавить всплывающий текст "+15$" здесь
	}

	// --- СИСТЕМА УЛУЧШЕНИЙ (Копия логики из Turret) ---

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
			if (upgradeInfoText) upgradeInfoText.text = $"Income: {currentIncome}\nRate: {currentCycleTime:F1}s";
		}
		else
		{
			FarmUpgradeStep next = upgrades[currentLevel];
			upgradeCostText.text = next.price.ToString();
			upgradeButton.interactable = true;

			if (upgradeInfoText)
				upgradeInfoText.text = $"+{next.incomeBonus} Gold\n-{next.speedBonus}s Time";
		}
	}

	private void Upgrade()
	{
		if (currentLevel >= upgrades.Length) return;

		FarmUpgradeStep step = upgrades[currentLevel];

		if (LevelManager.main.currency < step.price)
		{
			Debug.Log("Нет денег на улучшение фермы!");
			return;
		}

		// Тратим деньги
		LevelManager.main.SpentCurrency(step.price);
		if (StatsManager.main) StatsManager.main.TrackMoneySpent(step.price);

		// Применяем улучшения
		currentIncome += step.incomeBonus;
		currentCycleTime -= step.speedBonus;

		// Защита от слишком быстрой фермы (не меньше 0.5 сек)
		if (currentCycleTime < 0.5f) currentCycleTime = 0.5f;

		// Обновляем спрайты
		if (step.newFarmSprites != null && step.newFarmSprites.Length > 0)
		{
			for (int i = 0; i < farmSprites.Length; i++)
			{
				if (i < step.newFarmSprites.Length && step.newFarmSprites[i] != null)
				{
					farmSprites[i].sprite = step.newFarmSprites[i];
				}
			}
		}

		currentLevel++;
		UpdateUI();
		Debug.Log($"Ферма улучшена! Доход: {currentIncome}, Время: {currentCycleTime}");
	}
}