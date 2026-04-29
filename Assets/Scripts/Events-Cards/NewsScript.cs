using UnityEngine;
using UnityEngine.EventSystems;

public class NewsScript : MonoBehaviour, IPointerClickHandler
{
    // Esta función sustituye a tu OnMouseDown
    public void OnPointerClick(PointerEventData eventData)
    {
        Destroy(transform.parent.gameObject);
    }
}