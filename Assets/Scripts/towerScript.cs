using UnityEngine;

public class towerScript : MonoBehaviour
{
    /// <summary>
    /// Actúa como el radar de la torre. Comprueba frame a frame si un objeto 
    /// con la etiqueta "Enemy" permanece dentro de su zona de alcance.
    /// </summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            Debug.Log(" Detectado enemigo por " + this.gameObject.name);
    }
}
