using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreeSceneTransitionManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Escena")]
    [SerializeField] private string sceneToLoad = "Main";

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip fadeSound;

    [Header("Objetos a ocultar")]
    [SerializeField] private GameObject nodesObject;

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

    public void StartGameWithFade()
    {
        if (isTransitioning) return;

        // Ocultamos los nodos y los LineRenderer
        if (nodesObject != null)
        {
            nodesObject.SetActive(false);
        }

        // Sonido
        if (sfxSource != null && fadeSound != null)
        {
            sfxSource.PlayOneShot(fadeSound);
        }

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

        SceneManager.LoadScene(sceneToLoad);
    }
}