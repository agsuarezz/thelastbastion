using UnityEngine;
using UnityEngine.EventSystems;
public class CloseAdTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("Configuración")]
    public GameObject adObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        CloseAd();
    }

    private void CloseAd()
    {
        if (adObject != null)
        {
            Destroy(adObject);
        }
        else
        {
            Debug.LogWarning("¡Jefe, no has arrastrado el anuncio a la variable adObject en el Inspector!");
        }
    }
}