using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Datos (Inyectados por el Spawner)")]
    public EnemyData enemyData;

    [Header("UI")]
    public Slider lifeSlider;

    public float currentLife;
    public float currentSpeed;

    private Transform[] pathWaypoints;
    private int currentWaypointIndex = 0;

    private bool isDead = false;
    public bool IsDead => isDead;

    // Controla si este enemigo debe mostrar la barra de vida o no
    private bool showLifeBar = true;

    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead) return;

        MoveAlongPath();
    }

    public void SetPath(Transform[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
    }

    public void SetLifeBarVisible(bool visible)
    {
        showLifeBar = visible;

        if (lifeSlider != null)
        {
            lifeSlider.gameObject.SetActive(visible);
        }
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

    public float GetPathProgress()
    {
        if (pathWaypoints == null || pathWaypoints.Length == 0)
            return 0f;

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

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentLife -= (damageAmount * GameManager.globalDamageTakenMultiplier).ConvertTo<int>();

        if (lifeSlider != null)
        {
            lifeSlider.value = Mathf.Max(currentLife, 0);
        }

        if (currentLife <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        GameManager.enemiesDestroyed += 1;

        if (enemyData != null)
        {
            GameManager.countMoney += enemyData.money * GameManager.globalMoneyMultiplier;
        }

        currentSpeed = 0f;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            DestroyEnemy();
        }
    }

    public void DestroyEnemy()
    {
        Spawner.enemiesAlive--;
        gameObject.SetActive(false);
    }

    public void OnDeathAnimationFinished()
    {
        DestroyEnemy();
    }

    private void OnEnable()
    {
        isDead = false;
        currentWaypointIndex = 0;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

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

        if (lifeSlider != null)
        {
            lifeSlider.gameObject.SetActive(showLifeBar);

            if (enemyData != null)
            {
                lifeSlider.maxValue = enemyData.health;
                lifeSlider.value = currentLife;
            }
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}