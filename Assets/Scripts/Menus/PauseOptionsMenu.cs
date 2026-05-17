using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseOptionsMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject volumePopup;
    [SerializeField] private Slider volumeSlider;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainAudioMixer;

    private bool isVolumePopupOpen = false;

    private const string MasterVolumeParameter = "MasterVolume";
    private const string MasterVolumePrefsKey = "MasterVolume";

    private void Start()
    {
        if (volumePopup != null)
            volumePopup.SetActive(false);

        float savedVolume = PlayerPrefs.GetFloat(MasterVolumePrefsKey, 0.75f);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0.0001f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVolume);
    }

    public void ToggleVolumePopup()
    {
        isVolumePopupOpen = !isVolumePopupOpen;

        if (volumePopup != null)
            volumePopup.SetActive(isVolumePopupOpen);
    }

    public void SetVolume(float volume)
    {
        float safeVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        float dbValue = Mathf.Log10(safeVolume) * 20f;

        if (mainAudioMixer != null)
            mainAudioMixer.SetFloat(MasterVolumeParameter, dbValue);

        PlayerPrefs.SetFloat(MasterVolumePrefsKey, safeVolume);
        PlayerPrefs.Save();
    }
}