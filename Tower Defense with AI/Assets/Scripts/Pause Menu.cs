using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject mainPausePanel;
	[SerializeField] private GameObject settingsPanel;  

	private bool isPaused = false;

	private void Start()
	{
		mainPausePanel.SetActive(false);
		settingsPanel.SetActive(false);
	}

	public void PauseGame()
	{
		isPaused = true;
		UIManager.main.SetHoveringState(true);
		mainPausePanel.SetActive(true);
		settingsPanel.SetActive(false);
		Canvas.ForceUpdateCanvases();

		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		isPaused = false;
		UIManager.main.SetHoveringState(false);
		mainPausePanel.SetActive(false);
		settingsPanel.SetActive(false);
		Time.timeScale = 1f;
	}

	public void SetPauseByBtn()
	{
		if (isPaused)
			ResumeGame();
		else
			PauseGame();
	}

	public void OnContinueBtnClick()
	{
		ResumeGame();
	}

	public void OnSettingsBtnClick()
	{
		mainPausePanel.SetActive(false);

		settingsPanel.SetActive(true);
	}

	public void OnBackFromSettings()
	{
		AudioController.Instance.SaveVolumeSettings();
		settingsPanel.SetActive(false);
		mainPausePanel.SetActive(true);
	}

	public void OnRestartBtnClick()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void OnExitBtnClick()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}
}