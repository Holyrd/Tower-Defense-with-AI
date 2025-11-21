using UnityEngine;

public class Plot : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private SpriteRenderer sr;
	[SerializeField] private Color hoverColor;

	public GameObject towerObj;
	public Turret turret;
	private Color startColor;
	private bool TowerHasUpgrade = false;

	private void Start()
	{
		startColor = sr.color;
	}

	private void OnMouseEnter()
	{
		// ≈сли башн€ уже стоит, ничего не делаем (оставл€ем прозрачным)
		if (towerObj != null) return;

		sr.color = hoverColor;
	}

	private void OnMouseExit()
	{
		// ≈сли башн€ стоит, не возвращаем старый цвет (пусть остаетс€ прозрачным)
		if (towerObj != null) return;

		sr.color = startColor;
	}

	private void OnMouseDown()
	{
		if(UIManager.main.IsHoveringUI()) {return;}

		if (towerObj != null)
		{
			if (TowerHasUpgrade)
			{
				turret.OpenUpgradeUI();
			}
			return;
		}

		Tower towerToBuild = BuildManager.main.GetSelectedTower();

		if (towerToBuild.cost > LevelManager.main.currency)
		{
			Debug.Log("No Money");
			return;
		}

		LevelManager.main.SpentCurrency(towerToBuild.cost);

		towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
		sr.color = Color.clear;
		if (towerObj.GetComponent<Turret>() != null)
		{
			turret = towerObj.GetComponent<Turret>();
			TowerHasUpgrade = true;
		}
	}
}
