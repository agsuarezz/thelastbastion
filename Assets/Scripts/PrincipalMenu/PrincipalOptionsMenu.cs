using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrincipalOptionsMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private TextMeshProUGUI musicButtonText;

    [Header("Icono de música")]
    [SerializeField] private Image musicIcon;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;

    private bool isOpen = false;
    private bool musicEnabled = true;

    private void Start()
    {
        optionsPanel.SetActive(false);

        if (menuMusic != null)
        {
            musicEnabled = menuMusic.isPlaying;
        }

        UpdateMusicUI();
    }

    public void ToggleOptionsMenu()
    {
        isOpen = !isOpen;

        optionsPanel.SetActive(isOpen);

        Time.timeScale = isOpen ? 0f : 1f;
    }

    public void Resume()
    {
        isOpen = false;
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        if (menuMusic != null)
        {
            if (musicEnabled)
                menuMusic.Play();
            else
                menuMusic.Pause();
        }

        UpdateMusicUI();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateMusicUI()
    {
        if (musicButtonText != null)
        {
            musicButtonText.text = musicEnabled ? "Música: ON" : "Música: OFF";
        }

        if (musicIcon != null)
        {
            musicIcon.sprite = musicEnabled ? musicOnSprite : musicOffSprite;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}