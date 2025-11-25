using TMPro;
using UnityEngine;

public class Stats : MonoBehaviour
{
	[Header("References")]
	[SerializeField] TextMeshProUGUI currencyUI;
	[SerializeField] TextMeshProUGUI currencyWave;
	[SerializeField] TextMeshProUGUI currencyLives;

	private void OnGUI()
	{
		currencyUI.text = LevelManager.main.currency.ToString();
		currencyWave.text = LevelManager.main.wave.ToString();
		currencyLives.text = LevelManager.main.lives.ToString();
	}
}
