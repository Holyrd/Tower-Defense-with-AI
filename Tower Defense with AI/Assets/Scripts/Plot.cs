using UnityEngine;

public class Plot : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private SpriteRenderer sr;
	[SerializeField] private Color hoverColor;

	public GameObject towerObj;
	public Turret turret;
	private Color startColor;
	private Collider2D plotCollider; // Ссылка на коллайдер

	private void Awake() // Используем Awake чтобы найти коллайдер
	{
		plotCollider = GetComponent<Collider2D>();
	}

	public bool HasTower()
	{
		return towerObj != null;
	}

	public void BecomePath()
	{
		// Если здесь уже стоит башня - ничего не делаем, это не дорога
		if (HasTower()) return;

		if (plotCollider != null) plotCollider.enabled = false;
		sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Цвет пути
	}

	public void BecomeBuildable()
	{
		// Если здесь стоит башня - не включаем белый цвет и коллайдер, 
		// чтобы не перекрыть башню
		if (HasTower()) return;

		if (plotCollider != null) plotCollider.enabled = true;
		sr.color = Color.clear; // Возвращаем прозрачность
	}

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
		if(UIManager.main.IsHoveringUI()) {return;}

		if (towerObj != null)
		{
			return;
		}

		Tower towerToBuild = BuildManager.main.GetSelectedTower();

		if (towerToBuild.cost > LevelManager.main.currency)
		{
			Debug.Log("No Money");
			return;
		}

		LevelManager.main.SpentCurrency(towerToBuild.cost);

		StatsManager.main.TrackMoneySpent(towerToBuild.cost);
		StatsManager.main.TrackTowerBuilt();

		towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
		sr.color = Color.clear;

		Pathfinder.main.ScanGrid();

		if (towerObj.GetComponent<Turret>() != null)
		{
			turret = towerObj.GetComponent<Turret>();
		}
	}
}
