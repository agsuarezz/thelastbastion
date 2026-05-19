using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class TooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipGameObject;
    // Esta función salta automáticamente cuando el ratón ENTRA en el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(tooltipGameObject != null)
        {
            tooltipGameObject.SetActive(true);
        }
    }

    // Esta función salta automáticamente cuando el ratón SALE del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipGameObject != null)
        {
            tooltipGameObject.SetActive(false);
        }
    }
}

