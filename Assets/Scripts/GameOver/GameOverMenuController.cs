using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "principalMenu";
    [SerializeField] private string statsSceneName = "GameOverStats";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Main Menu Transition")]
    [SerializeField] private AudioClip menuFadeSound;
    [SerializeField] private float menuFadeDuration = 1f;

    [Header("Stats Transition")]
    [SerializeField] private AudioClip statsFadeSound;
    [SerializeField] private float statsFadeDuration = 1f;

    private bool isTransitioning = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void GoToMainMenu()
    {
        StartFadeToScene(
            mainMenuSceneName,
            menuFadeSound,
            menuFadeDuration
        );
    }

    public void GoToStats()
    {
        StartFadeToScene(
            statsSceneName,
            statsFadeSound,
            statsFadeDuration
        );
    }

    private void StartFadeToScene(
        string targetScene,
        AudioClip soundToPlay,
        float fadeDuration)
    {
        if (isTransitioning) return;

        if (sfxSource != null && soundToPlay != null)
        {
            sfxSource.PlayOneShot(soundToPlay);
        }

        StartCoroutine(
            FadeAndLoadScene(
                targetScene,
                fadeDuration
            )
        );
    }

    private IEnumerator FadeAndLoadScene(
        string targetScene,
        float fadeDuration)
    {
        isTransitioning = true;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;

                fadeCanvasGroup.alpha =
                    Mathf.Clamp01(elapsedTime / fadeDuration);

                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(targetScene);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}