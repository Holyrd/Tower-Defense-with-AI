using UnityEngine;
using TMPro; // Обязательно для TextMeshPro

public class ShopTowerUI : MonoBehaviour
{
	[Header("Настройки")]
	[Tooltip("Индекс башни в массиве BuildManager (0 - первая, 1 - вторая и т.д.)")]
	public int towerIndex;

	[Header("UI Компоненты")]
	public TextMeshProUGUI costText; // Перетяни сюда текст с ценой

	private void Start()
	{
		// Обновляем цену при старте игры
		UpdateCostText();
		if (BuildManager.main != null)
			BuildManager.main.OnCostChanged += UpdateCostText;
	}

	private void UpdateCostText()
	{
		if (BuildManager.main == null) return;

		Tower t = BuildManager.main.GetTower(towerIndex);
		if (t != null)
		{
			costText.text = t.cost.ToString(); // Или t.cost + "$"
		}
	}
}