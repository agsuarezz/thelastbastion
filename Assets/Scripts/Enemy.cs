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

    private Transform[] pathWaypoints;
    private int currentWaypointIndex = 0;
    private bool isDead = false;

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
        MoveAlongPath();
    }

    public void SetPath(Transform[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
    }

    private void MoveAlongPath()
    {
        if (pathWaypoints == null || currentWaypointIndex >= pathWaypoints.Length) return;

        Transform targetWaypoint = pathWaypoints[currentWaypointIndex];

        Vector3 direction = targetWaypoint.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, targetWaypoint.position) <= 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    /// <summary>
    /// Resta vida al enemigo y actualiza su barra. Si llega a 0, se destruye.
    /// Suma uno al contador de enemigos Derrotados
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        currentLife -= damageAmount;
 
        if (lifeSlider != null)
        {
            lifeSlider.value = currentLife;
        }

        if (currentLife <= 0 && !isDead)
        {
            isDead = true;
            GameManager.enemiesDestroyed += 1;
            GameManager.countMoney += 10;
            DestroyEnemy();
        }
    }

    
    /// <summary>
    /// Destruye a este mismo enemigo. y Resta uno al numero de enemigo en escena
    /// </summary>
    public void DestroyEnemy()
    {
        Spawner.enemiesAlive--;
        Destroy(gameObject);
    }
}