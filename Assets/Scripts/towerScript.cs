using UnityEngine;

public class towerScript : MonoBehaviour
{
    // Es de prueba para verificar que funcionaba las colisiones de las torres si metes un boxCollider2D
    // Si lo quiere utilizar pueden.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            Debug.Log("detectado enemigo");
    }
}
