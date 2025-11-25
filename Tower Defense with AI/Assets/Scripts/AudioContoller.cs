using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
	public static AudioController Instance;

	[Header("Assets")]
	[SerializeField] private Sprite musicOff; 
	[SerializeField] private Sprite musicOn;  
	[SerializeField] private AudioSource audioVol;

	private Slider linkedSlider;
	private Image linkedImage;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		AudioListener.volume = PlayerPrefs.GetFloat("musicOnOff", 1f);
		if (audioVol != null)
		{
			audioVol.volume = PlayerPrefs.GetFloat("musicVolume", 0.5f);
		}
	}

	public void ConnectUI(Slider slider, Image img)
	{
		linkedSlider = slider;
		linkedImage = img;

		if (linkedSlider != null)
		{
			linkedSlider.value = audioVol.volume;
			linkedSlider.onValueChanged.RemoveAllListeners();
			linkedSlider.onValueChanged.AddListener(SetVolume);
		}

		UpdateIcon();
	}

	public void SetVolume(float value)
	{
		audioVol.volume = value;
	}

	public void SetMuteState(bool isSoundOn)
	{
		if (isSoundOn)
			AudioListener.volume = 1;
		else
			AudioListener.volume = 0;

		UpdateIcon();       
		SaveVolumeSettings(); 
	}

	public void UpdateIcon()
	{
		if (linkedImage == null) return;

		if (AudioListener.volume > 0.01f)
			linkedImage.sprite = musicOn;
		else
			linkedImage.sprite = musicOff;
	}

	public void SaveVolumeSettings()
	{
		PlayerPrefs.SetFloat("musicVolume", audioVol.volume);
		PlayerPrefs.SetFloat("musicOnOff", AudioListener.volume);
		PlayerPrefs.Save();
	}
}