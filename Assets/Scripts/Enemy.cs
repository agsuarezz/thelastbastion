using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Controla el comportamiento básico del enemigo, gestionando su movimiento y su destrucción.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Tooltip("Velocidad a la que se desplaza el enemigo.")]
    public float speed = 2f;

    [Tooltip("Vida del enemigo.")]
    public int maxLife = 100;

    [Tooltip("Barra de vida de la interfaz (UI).")]
    public Slider lifeSlider;

    // Estado interno de la vida actual del enemigo
    private int currentLife;
    /// <summary>
    /// Mueve al enemigo hacia la derecha de la pantalla de forma continua y fluida.
    /// </summary>
    private void Start()
    {
        currentLife = maxLife;

        if (lifeSlider != null)
        {
            lifeSlider.maxValue = maxLife;
            lifeSlider.value = currentLife;
        }
    }
    
    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    /// <summary>
    /// Resta vida al enemigo y actualiza su barra. Si llega a 0, se destruye.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        currentLife -= damageAmount;
 
        if (lifeSlider != null)
        {
            lifeSlider.value = currentLife;
        }

        if (currentLife <= 0)
        {
            DestroyEnemy();
        }
    }

    
    /// <summary>
    /// Destruye a este mismo enemigo.
    /// </summary>
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}