using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public static LevelManager main;

	public int currency { get; private set; }
	public int wave { get; private set; }
	public int lives { get; private set; }
	public Transform startPoint;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		currency = 200;
		wave = 1;
		lives = 100;
	}

	public void IncreasCurrency(int amount)
	{
		currency += amount;
	}

	public bool SpentCurrency(int amount)
	{
		if (currency >= amount)
		{
			currency -= amount;
			return true;
		}
		else
		{
			Debug.Log("No monye");
			return false;
		}
	}

	public void SetWave(int _wave)
	{
		wave = _wave;
	}

	public void GetDamage(int _damage)
	{
		if (PerformanceMonitor.instance != null)
			PerformanceMonitor.instance.RegisterHealthLoss(_damage);

		if (_damage >= lives)
		{
			lives = 0;
			WinLosePanel.main.EndGame(false);
		}
		else
		{
			lives -= _damage;
		}
	}
}
