using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverStatsController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameOverSceneName = "GameOverPrueba";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

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

    public void GoBackToGameOver()
    {
        if (isTransitioning) return;

        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
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

        SceneManager.LoadScene(gameOverSceneName);
    }
}