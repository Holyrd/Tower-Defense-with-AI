using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinLosePanel : MonoBehaviour
{
	public static WinLosePanel main;

	[Header("References")]
	[SerializeField] private GameObject panelObject; 
	[SerializeField] private Image resultImage;      

	[Header("Sprites")]
	[SerializeField] private Sprite winImg;
	[SerializeField] private Sprite loseImg;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		if (panelObject != null)
			panelObject.SetActive(false);
	}

	public void EndGame(bool isWin)
	{
		panelObject.SetActive(true);

		var stats = StatsManager.main.currentStats;

		Debug.Log($"Игра окончена! Убито: {stats.enemiesKilled}, Урон: {stats.totalDamageDealt}");

		if (isWin)
		{
			resultImage.sprite = winImg;
			LevelComplete();
		}
		else
		{
			resultImage.sprite = loseImg;
		}

		Time.timeScale = 0f;
	}

	private void LevelComplete()
	{
		int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

		int savedLevel = PlayerPrefs.GetInt("levelReached", 1);

		if (currentLevelIndex >= savedLevel)
		{
			PlayerPrefs.SetInt("levelReached", currentLevelIndex + 1);
			PlayerPrefs.Save();
			Debug.Log("Рівень пройдено! Відкрит рівень: " + (currentLevelIndex + 1));
		}
	}

	public void RestartLevel()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void ToMenu()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}
}
