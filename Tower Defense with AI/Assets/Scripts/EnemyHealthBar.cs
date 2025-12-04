using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Health healthScript; 
	[SerializeField] private Image fillImage;     
	[SerializeField] private GameObject canvasObject;  

	private void OnEnable()
	{
		if (healthScript != null)
			healthScript.OnHealthChanged += UpdateHealthBar;

		UpdateHealthBar(1f);
	}

	private void OnDisable()
	{
		if (healthScript != null)
			healthScript.OnHealthChanged -= UpdateHealthBar;
	}

	private void UpdateHealthBar(float pct)
	{
		fillImage.fillAmount = pct;
	}

	private void LateUpdate()
	{
		transform.rotation = Quaternion.identity;
	}
}