using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class principalSceneTransition : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Escena a cargar")]
    public string sceneToLoad = "Main";

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip fadeSound;

    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void StartGameWithFade()
    {
        if (!isTransitioning)
        {
            // 🔊 sonido sincronizado con el fade
            if (sfxSource != null && fadeSound != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f); // opcional, queda más natural
                sfxSource.PlayOneShot(fadeSound);
            }

            StartCoroutine(FadeAndLoadScene());
        }
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
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}