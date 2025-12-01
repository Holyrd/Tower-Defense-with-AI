using UnityEngine;
using TMPro; // Обязательно для TextMeshPro

public class DifficultyDropdownHandler : MonoBehaviour
{
	[Header("Компонент Dropdown")]
	public TMP_Dropdown difficultyDropdown;

	private const string PREF_KEY = "SelectedDifficulty";

	private void Start()
	{
		// 1. При старте узнаем, что было выбрано раньше (по умолчанию 1 - Нормально)
		int savedIndex = PlayerPrefs.GetInt(PREF_KEY, 1);

		// 2. Ставим это значение в меню (без звука щелчка)
		difficultyDropdown.SetValueWithoutNotify(savedIndex);

		// 3. Следим за изменениями: если игрок выберет другое, сработает OnDifficultyChanged
		difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
	}

	private void OnDifficultyChanged(int index)
	{
		// Сохраняем выбор: 0 = Easy, 1 = Normal, 2 = Hard
		PlayerPrefs.SetInt(PREF_KEY, index);
		PlayerPrefs.Save();

		Debug.Log($"Сложность изменена на: {index}");
	}

	private void OnDestroy()
	{
		// Убираем слежку при удалении объекта (во избежание ошибок)
		if (difficultyDropdown != null)
		{
			difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
		}
	}
}