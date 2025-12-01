using UnityEngine;
using UnityEngine.UI;

public class DifficultyIndicator : MonoBehaviour
{
	[Header("Компоненты")]
	public Image statusImage; // Сюда перетяни компонент Image с этого же объекта

	[Header("Спрайты состояний")]
	public Sprite iconNormal; // Серая полоска
	public Sprite iconHard;   // Зеленая стрелка ВВЕРХ (Игрок крут -> сложность растет)
	public Sprite iconEasy;   // Красная стрелка ВНИЗ (Игрок слаб -> сложность падает)

	private void Start()
	{
		// Подписываемся на события менеджера
		if (DynamicDifficultyManager.instance != null)
		{
			DynamicDifficultyManager.instance.OnDifficultyChanged += UpdateVisuals;

			// Сразу обновляем картинку при старте (вдруг мы загрузили игру где уже сложно)
			UpdateVisuals(DynamicDifficultyManager.instance.GetCurrentState());
		}
	}

	private void OnDestroy()
	{
		// Обязательно отписываемся, чтобы не было ошибок при перезагрузке сцены
		if (DynamicDifficultyManager.instance != null)
		{
			DynamicDifficultyManager.instance.OnDifficultyChanged -= UpdateVisuals;
		}
	}

	private void UpdateVisuals(DynamicDifficultyManager.DDAState state)
	{
		switch (state)
		{
			case DynamicDifficultyManager.DDAState.Hard:
				if (iconHard) statusImage.sprite = iconHard;
				statusImage.color = Color.white; // Можно менять цвет кодом, если нет спрайтов
				break;

			case DynamicDifficultyManager.DDAState.Easy:
				if (iconEasy) statusImage.sprite = iconEasy;
				break;

			case DynamicDifficultyManager.DDAState.Normal:
			default:
				if (iconNormal) statusImage.sprite = iconNormal;
				break;
		}
	}
}