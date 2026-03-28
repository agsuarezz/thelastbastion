using System.Collections;
using UnityEngine;
/// <summary>
/// Controla el comportamiento básico del enemigo, gestionando su movimiento y su destrucción.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Tooltip("Velocidad a la que se desplaza el enemigo.")]
    public float speed = 2f;
    [Tooltip("Vida del enemigo.")]
    public int lifeEnemy;
    /// <summary>
    /// Mueve al enemigo hacia la derecha de la pantalla de forma continua y fluida.
    /// </summary>
    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
    /// <summary>
    /// Destruye a este mismo enemigo.
    /// </summary>
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}