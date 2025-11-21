using UnityEngine;

public class LevelManager : MonoBehaviour
{
	public static LevelManager main;

	public int currency { get; private set; }
	public Transform startPoint;
	public Transform[] path;

	

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		currency = 1000;
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
}
