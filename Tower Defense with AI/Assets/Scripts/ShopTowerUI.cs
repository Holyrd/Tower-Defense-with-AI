using UnityEngine;
using TMPro; 

public class ShopTowerUI : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Tower index in BuildManager (0 - first, 1 - second etc.)")]
	public int towerIndex;

	[Header("UI Components")]
	public TextMeshProUGUI costText;

	private void Start()
	{
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
			costText.text = t.cost.ToString();
		}
	}
}