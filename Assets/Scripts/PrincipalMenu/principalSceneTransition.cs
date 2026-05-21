using System.Collections;
using TMPro;
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
    public AudioClip treeFadeSound;

    private bool isTransitioning = false;

    public TextMeshProUGUI buttonStartText;

    private void Start()
    {
        if (buttonStartText != null)
        {
            buttonStartText.text = (SaveSystem.SaveExists()) ? "CONTINUAR" : "NUEVA PARTIDA";
            buttonStartText.fontSize = (SaveSystem.SaveExists()) ? 12 : 10;
        }

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
        StartFadeToScene(sceneToLoad, fadeSound);
    }

    public void goToTree()
    {
        StartFadeToScene("Tree", treeFadeSound);
    }

    private void StartFadeToScene(string targetScene, AudioClip soundToPlay)
    {
        if (isTransitioning) return;

        if (sfxSource != null && soundToPlay != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(soundToPlay);
        }

        StartCoroutine(FadeAndLoadScene(targetScene));
    }

    private IEnumerator FadeAndLoadScene(string targetScene)
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

        SceneManager.LoadScene(targetScene);
    }
}