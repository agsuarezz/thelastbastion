using UnityEngine;

public class CloseAdTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject adObject;


    // Se activa cuando el jugador hace clic izquierdo en el BoxCollider de este objeto
    private void OnMouseDown()
    {
        CloseAd();
    }

    private void CloseAd()
    {
        
        if (adObject != null)
        {
            Destroy(adObject);
            Debug.Log("XD");
        }
        else
        {
            Debug.LogWarning("¡Jefe, no has arrastrado el anuncio a la variable adObject en el Inspector!");
        }
    }
}