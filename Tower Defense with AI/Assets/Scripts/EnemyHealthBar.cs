using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
	[Header("Ссылки")]
	[SerializeField] private Health healthScript; // Ссылка на скрипт здоровья врага
	[SerializeField] private Image fillImage;     // Ссылка на зеленую картинку
	[SerializeField] private GameObject canvasObject; // Весь канвас (чтобы скрыть, если HP полное)

	private void OnEnable()
	{
		// Подписываемся на событие
		if (healthScript != null)
			healthScript.OnHealthChanged += UpdateHealthBar;

		// При появлении врага делаем полоску полной
		UpdateHealthBar(1f);
	}

	private void OnDisable()
	{
		// Отписываемся, чтобы не было ошибок
		if (healthScript != null)
			healthScript.OnHealthChanged -= UpdateHealthBar;
	}

	private void UpdateHealthBar(float pct)
	{
		// Меняем заполнение картинки
		fillImage.fillAmount = pct;

		// (Опционально) Скрываем полоску, если здоровье полное, и показываем, если ранен
		// canvasObject.SetActive(pct < 1f); 

	}

	// (Опционально) Чтобы полоска не вращалась вместе с врагом, если он поворачивается
	private void LateUpdate()
	{
		transform.rotation = Quaternion.identity; // Всегда держим ровно
	}
}