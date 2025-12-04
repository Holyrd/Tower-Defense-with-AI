using UnityEngine;
using System; 

public class BuildManager : MonoBehaviour
{
	public static BuildManager main;

	[Header("References")]
	[SerializeField] private Tower[] towers;

	public event Action OnCostChanged;

	private int selectedTower = 0;

	private void Awake()
	{
		main = this;
		foreach (Tower t in towers)
		{
			t.baseCost = t.cost;
		}
	}

	public Tower GetSelectedTower()
	{
		return towers[selectedTower];
	}

	public Tower GetTower(int index)
	{
		if (index < 0 || index >= towers.Length) return null;
		return towers[index];
	}

	public void SetSelectedTower(int _selectedTower)
	{
		selectedTower = _selectedTower;
	}

	public void IncreaseTowerCost(Tower towerToAdjust)
	{
		towerToAdjust.cost += towerToAdjust.baseCost;

		if (towerToAdjust.cost >= 500)
		{
			towerToAdjust.cost = 500;
		}

		OnCostChanged?.Invoke();
	}
}