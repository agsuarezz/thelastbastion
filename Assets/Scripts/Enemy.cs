using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el comportamiento básico del enemigo, gestionando su movimiento y su destrucción.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Datos (Inyectados por el Spawner)")]
    [Tooltip("El ScriptableObject con las estadísticas base (Vida, Daño, Velocidad).")]
    public EnemyData enemyData;

    [Header("UI")]
    [Tooltip("Barra de vida de la interfaz (UI).")]
    public Slider lifeSlider;

    // Estado de la vida y speed actual del enemigo
    public float currentLife;
    public float currentSpeed;

    private Transform[] pathWaypoints;
    private int currentWaypointIndex = 0;

    private bool isDead = false;

    private void Update()
    {
        MoveAlongPath();
    }

    public void SetPath(Transform[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            return;
    }

    private void MoveAlongPath()
    {
        if (pathWaypoints == null || currentWaypointIndex >= pathWaypoints.Length) return;

        Transform targetWaypoint = pathWaypoints[currentWaypointIndex];

        Vector3 direction = targetWaypoint.position - transform.position;
        transform.Translate(direction.normalized * currentSpeed * GameManager.globalSpeedMultiplier * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, targetWaypoint.position) <= 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    /// <summary>
    /// Devuelve cuánto ha avanzado este enemigo por la ruta.
    /// Cuanto mayor sea el valor, más cerca está del final.
    /// </summary>
    public float GetPathProgress()
    {
        if (pathWaypoints == null || pathWaypoints.Length == 0)
            return 0f;

        // Si ya pasó todos los waypoints, es el más adelantado posible
        if (currentWaypointIndex >= pathWaypoints.Length)
            return pathWaypoints.Length;

        float progress = currentWaypointIndex;

        Transform targetWaypoint = pathWaypoints[currentWaypointIndex];
        Transform previousWaypoint = currentWaypointIndex > 0 ? pathWaypoints[currentWaypointIndex - 1] : null;

        if (previousWaypoint != null)
        {
            float segmentLength = Vector2.Distance(previousWaypoint.position, targetWaypoint.position);
            float distanceFromPrevious = Vector2.Distance(previousWaypoint.position, transform.position);

            if (segmentLength > 0f)
            {
                progress += Mathf.Clamp01(distanceFromPrevious / segmentLength);
            }
        }

        return progress;
    }

    /// <summary>
    /// Resta vida al enemigo y actualiza su barra. Si llega a 0, se destruye.
    /// Suma uno al contador de enemigos Derrotados
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        currentLife -= (damageAmount * GameManager.globalDamageTakenMultiplier).ConvertTo<int>();
        if (lifeSlider != null)
        {
            lifeSlider.value = currentLife;
        }

        if (currentLife <= 0 && !isDead)
        {
            isDead = true;
            GameManager.enemiesDestroyed += 1;
            GameManager.countMoney += enemyData.money * GameManager.globalMoneyMultiplier;
            DestroyEnemy();
        }
    }

    /// <summary>
    /// Destruye a este mismo enemigo. y Resta uno al numero de enemigo en escena
    /// </summary>
    public void DestroyEnemy()
    {
        Spawner.enemiesAlive--;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        isDead = false;

        if (enemyData != null)
        {
            currentLife = enemyData.health;
            currentSpeed = enemyData.speed;
        }
        else
        {
            currentLife = 100f;
            currentSpeed = 1.5f;
        }

        if (lifeSlider != null && enemyData != null)
        {
            lifeSlider.maxValue = enemyData.health;
            lifeSlider.value = currentLife;
        }
    }
}