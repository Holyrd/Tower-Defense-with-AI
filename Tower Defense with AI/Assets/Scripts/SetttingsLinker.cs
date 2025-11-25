using UnityEngine;
using UnityEngine.UI;

public class SettingsLinker : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField] private Slider settingsSlider;
	[SerializeField] private Toggle soundToggle;     
	[SerializeField] private Image targetIconImage;  

	private void OnEnable()
	{
		if (AudioController.Instance != null)
		{
			AudioController.Instance.ConnectUI(settingsSlider, targetIconImage);

			if (soundToggle != null)
			{
				bool isSoundOn = AudioListener.volume > 0.01f;

				soundToggle.SetIsOnWithoutNotify(isSoundOn);

				soundToggle.onValueChanged.RemoveAllListeners();
				soundToggle.onValueChanged.AddListener(OnToggleChanged);
			}
		}
	}

	private void OnToggleChanged(bool isEnabled)
	{
		if (AudioController.Instance != null)
		{
			AudioController.Instance.SetMuteState(isEnabled);
		}
	}
}