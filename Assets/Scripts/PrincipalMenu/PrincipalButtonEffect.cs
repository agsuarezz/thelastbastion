using UnityEngine;
using UnityEngine.EventSystems;

public class PrincipalButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Escala")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float speed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
}