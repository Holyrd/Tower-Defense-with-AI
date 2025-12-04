using UnityEngine;
using UnityEngine.UI;

public class DifficultyIndicator : MonoBehaviour
{
	[Header("Componet")]
	public Image statusImage; 

	[Header("Status Sprites")]
	public Sprite iconNormal; 
	public Sprite iconHard;   
	public Sprite iconEasy;   

	private void Start()
	{
		if (DynamicDifficultyManager.instance != null)
		{
			DynamicDifficultyManager.instance.OnDifficultyChanged += UpdateVisuals;

			UpdateVisuals(DynamicDifficultyManager.instance.GetCurrentState());
		}
	}

	private void OnDestroy()
	{
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
				statusImage.color = Color.white; 
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