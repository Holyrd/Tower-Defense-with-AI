using UnityEngine;

public class Plot : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private SpriteRenderer sr;
	[SerializeField] private Color hoverColor;

	public GameObject towerObj;
	public Turret turret;
	private Color startColor;
	private Collider2D plotCollider;

	private void Awake()
	{
		plotCollider = GetComponent<Collider2D>();
	}

	// ... (методы HasTower, BecomePath, BecomeBuildable оставляем без изменений) ...
	public bool HasTower() { return towerObj != null; }

	public void BecomePath()
	{
		if (HasTower()) return;
		if (plotCollider != null) plotCollider.enabled = false;
		sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	}

	public void BecomeBuildable()
	{
		if (HasTower()) return;
		if (plotCollider != null) plotCollider.enabled = true;
		sr.color = Color.clear;
	}
	// ... (конец блока неизменных методов) ...

	private void Start()
	{
		startColor = Color.clear;
		sr.color = Color.clear;
	}

	private void OnMouseEnter()
	{
		if (towerObj != null) return;
		sr.color = hoverColor;
	}

	private void OnMouseExit()
	{
		if (towerObj != null) return;
		sr.color = startColor;
	}

	private void OnMouseDown()
	{
		if (UIManager.main.IsHoveringUI()) { return; }

		if (towerObj != null) return;

		Tower towerToBuild = BuildManager.main.GetSelectedTower();

		// 1. Проверяем цену (она будет уже обновленной)
		if (towerToBuild.cost > LevelManager.main.currency)
		{
			Debug.Log("No Money");
			return;
		}

		// 2. Тратим деньги
		LevelManager.main.SpentCurrency(towerToBuild.cost);

		StatsManager.main.TrackMoneySpent(towerToBuild.cost);
		StatsManager.main.TrackTowerBuilt();

		// 3. Строим
		towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
		sr.color = Color.clear;

		// --- НОВОЕ: Увеличиваем цену этой башни для следующей покупки ---
		BuildManager.main.IncreaseTowerCost(towerToBuild);
		// ---------------------------------------------------------------

		Pathfinder.main.ScanGrid();

		if (towerObj.GetComponent<Turret>() != null)
		{
			turret = towerObj.GetComponent<Turret>();
		}
	}
}