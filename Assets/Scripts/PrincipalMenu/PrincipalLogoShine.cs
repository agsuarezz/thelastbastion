using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PrincipalLogoShine : MonoBehaviour
{
    [SerializeField] private RectTransform shineTransform;
    [SerializeField] private Image shineImage;

    [SerializeField] private float startX = -450f;
    [SerializeField] private float endX = 450f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float delayBetweenShines = 4f;

    private Color originalColor;

    private void Awake()
    {
        if (shineImage == null)
            shineImage = shineTransform.GetComponent<Image>();

        originalColor = shineImage.color;

        SetShineVisible(false);
        MoveShineToStart();
    }

    private void Start()
    {
        StartCoroutine(ShineLoop());
    }

    private IEnumerator ShineLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBetweenShines);

            MoveShineToStart();
            SetShineVisible(true);

            Vector2 startPos = shineTransform.anchoredPosition;
            startPos.x = startX;

            Vector2 endPos = startPos;
            endPos.x = endX;

            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                shineTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            shineTransform.anchoredPosition = endPos;
            SetShineVisible(false);
        }
    }

    private void MoveShineToStart()
    {
        Vector2 position = shineTransform.anchoredPosition;
        position.x = startX;
        shineTransform.anchoredPosition = position;
    }

    private void SetShineVisible(bool visible)
    {
        Color color = originalColor;
        color.a = visible ? originalColor.a : 0f;
        shineImage.color = color;
    }
}