using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsPanel : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Button[] levelButtons;

	private void Start()
	{
		int levelReached = PlayerPrefs.GetInt("levelReached", 1);

		for (int i = 0; i < levelButtons.Length; i++)
		{
			int levelNum = i + 1;

			if (levelNum > levelReached)
			{
				levelButtons[i].interactable = false;
			}
			else
			{
				levelButtons[i].interactable = true;
			}
		}
	}

	//Test
	public void ResetProgress()
	{
		PlayerPrefs.DeleteAll();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
