using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private GameObject optionsPanel;
	[SerializeField] private GameObject levelPanel;


	public void StartPlay()
	{
		levelPanel.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void CloseStartPlay()
	{
		levelPanel.SetActive(false);
		menuPanel.SetActive(true);
	}

	public void QuitGame()
	{
		Debug.Log("Game closed");
		Application.Quit();
	}

	public void ChooseLevel(int level)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + level);
	}

	public void OpenOptions()
	{
		optionsPanel.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void CloseOptions()
	{
		AudioController.Instance.SaveVolumeSettings();
		optionsPanel.SetActive(false);
		menuPanel.SetActive(true);
	}
}
