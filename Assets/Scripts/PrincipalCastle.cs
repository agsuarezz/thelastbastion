using UnityEngine;

/// <summary>
/// Versión del castillo para el menú principal.
/// No tiene vida, no hay game over.
/// Solo destruye a los enemigos cuando llegan al final.
/// </summary>
public class principalCastle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemigo = other.GetComponent<Enemy>();

            if (enemigo != null)
            {
                enemigo.DestroyEnemy();
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}