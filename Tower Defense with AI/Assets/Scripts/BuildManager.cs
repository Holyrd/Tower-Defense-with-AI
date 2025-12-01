using UnityEngine;
using System; // НУЖНО для Action

public class BuildManager : MonoBehaviour
{
	public static BuildManager main;

	[Header("References")]
	[SerializeField] private Tower[] towers;

	// --- НОВОЕ: Событие изменения цены ---
	public event Action OnCostChanged;
	// ------------------------------------

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

	// --- НОВОЕ: Метод чтобы UI мог узнать информацию о любой башне по индексу ---
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
		// --- НОВОЕ: Сообщаем всем подписчикам, что цены изменились ---
		OnCostChanged?.Invoke();
		// -----------------------------------------------------------
	}
}