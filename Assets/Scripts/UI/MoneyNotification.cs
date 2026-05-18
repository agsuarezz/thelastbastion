using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textLabel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animación")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    public void Setup(int amount, bool isGain)
    {
        textLabel.text = isGain
            ? $"+{amount} oro"
            : $"-{amount} oro";

        textLabel.color = isGain
            ? new Color(1f, 0.85f, 0.2f)
            : new Color(1f, 0.35f, 0.35f);

        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        yield return new WaitForSeconds(duration);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            yield return null;
        }

        Destroy(gameObject);
    }
}