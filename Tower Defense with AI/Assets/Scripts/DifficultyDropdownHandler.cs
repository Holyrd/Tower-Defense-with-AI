using UnityEngine;
using TMPro;

public class DifficultyDropdownHandler : MonoBehaviour
{
	[Header("Component Dropdown")]
	public TMP_Dropdown difficultyDropdown;

	private const string PREF_KEY = "SelectedDifficulty";

	private void Start()
	{
		int savedIndex = PlayerPrefs.GetInt(PREF_KEY, 1);

		difficultyDropdown.SetValueWithoutNotify(savedIndex);

		difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
	}

	private void OnDifficultyChanged(int index)
	{
		PlayerPrefs.SetInt(PREF_KEY, index);
		PlayerPrefs.Save();

		Debug.Log($"Difficult changed: {index}");
	}

	private void OnDestroy()
	{
		if (difficultyDropdown != null)
		{
			difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
		}
	}
}