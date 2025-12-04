using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class FarmUpgradeStep
{
	[Header("Стоимость улучшения")]
	public int price;

	[Header("Бонусы")]
	public int incomeBonus;        
	public float speedBonus;       

	[Header("Визуал")]
	public Sprite[] newFarmSprites; 
}

public class Farm : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject incomeEffect;
	[SerializeField] private Transform incomePoint;

	[Header("UI References")]
	[SerializeField] private GameObject upgradeUI;
	[SerializeField] private Button upgradeButton;
	[SerializeField] TextMeshProUGUI upgradeCostText;
	[SerializeField] TextMeshProUGUI upgradeInfoText;

	[Header("Visuals")]
	[SerializeField] private SpriteRenderer[] farmSprites; 

	[Header("Base Stats")]
	[SerializeField] private int baseIncome = 15;      
	[SerializeField] private float baseCycleTime = 5f; 

	[Header("Upgrades")]
	public FarmUpgradeStep[] upgrades;

	private float timer;
	private int currentIncome;
	private float currentCycleTime;
	private int currentLevel = 0;

	private void Start()
	{
		currentIncome = baseIncome;
		currentCycleTime = baseCycleTime;
		timer = currentCycleTime;

		upgradeButton.onClick.AddListener(Upgrade);
		UpdateUI();
	}

	private void Update()
	{
		timer -= Time.deltaTime;

		if (timer <= 0f)
		{
			GenerateMoney();
			timer = currentCycleTime;
		}
	}

	private void GenerateMoney()
	{
		LevelManager.main.IncreasCurrency(currentIncome);

		if (StatsManager.main != null)
			StatsManager.main.TrackMoneyEarned(currentIncome);

		if (incomeEffect != null && incomePoint != null)
		{
			Instantiate(incomeEffect, incomePoint.position, Quaternion.identity);
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
			Debug.Log("No money to improve the farm!");
			return;
		}

		LevelManager.main.SpentCurrency(step.price);
		if (StatsManager.main) StatsManager.main.TrackMoneySpent(step.price);

		currentIncome += step.incomeBonus;
		currentCycleTime -= step.speedBonus;

		if (currentCycleTime < 0.5f) currentCycleTime = 0.5f;

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
		Debug.Log($"Farm improved! Income: {currentIncome}, Time: {currentCycleTime}");
	}
}