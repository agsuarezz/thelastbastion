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

    // 🔹 MANTENEMOS ESTO (LO USA EL SPAWNER)
    private bool showLifeBar = true;

    // 🔹 NUEVO: ATAQUE AL CASTILLO
    private bool isAttackingCastle = false;
    private float attackTimer = 0f;

    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private castleScript targetCastle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        targetCastle = FindObjectOfType<castleScript>();
    }

    private void Update()
    {
        if (isDead) return;

        if (isAttackingCastle)
        {
            AttackCastle();
            return;
        }

        MoveAlongPath();
    }

    public void SetPath(Transform[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
        isAttackingCastle = false;
        attackTimer = 0f;
    }

    // 🔹 NO LO QUITES (LO USA EL SPAWNER)
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
        if (pathWaypoints == null || pathWaypoints.Length == 0) return;

        // 🔹 CHECK DE RANGO AL CASTILLO
        if (targetCastle != null && targetCastle.castleCollider != null)
        {
            Vector2 closestPoint = targetCastle.castleCollider.ClosestPoint(transform.position);
            float distance = Vector2.Distance(transform.position, closestPoint);

            if (distance <= enemyData.attackRange)
            {
                StartAttackingCastle();
                return;
            }
        }

        if (currentWaypointIndex >= pathWaypoints.Length)
        {
            StartAttackingCastle();
            return;
        }

        Transform targetWaypoint = pathWaypoints[currentWaypointIndex];

        Vector3 direction = targetWaypoint.position - transform.position;
        transform.Translate(direction.normalized * currentSpeed * GameManager.globalSpeedMultiplier * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, targetWaypoint.position) <= 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    private void StartAttackingCastle()
    {
        if (isAttackingCastle) return;

        isAttackingCastle = true;
        currentSpeed = 0f;
        attackTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Si luego quieres animación:
        // animator.SetBool("IsAttacking", true);
    }

    private void AttackCastle()
    {
        if (targetCastle == null)
        {
            targetCastle = FindObjectOfType<castleScript>();
            if (targetCastle == null) return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            float damage = enemyData.damage * GameManager.globalEnemyDamageMultiplier;
            targetCastle.TakeDamage(Mathf.RoundToInt(damage));

            attackTimer = enemyData.attackCooldown;
        }
    }

    public float GetPathProgress()
    {
        if (pathWaypoints == null || pathWaypoints.Length == 0)
            return 0f;

        // 🔹 IMPORTANTE: enemigos atacando tienen prioridad máxima
        if (isAttackingCastle)
            return pathWaypoints.Length + 1f;

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
        isAttackingCastle = false;

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
        isAttackingCastle = false;
        currentWaypointIndex = 0;
        attackTimer = 0f;

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
            currentLife = enemyData.health * GameManager.globalEnemyHealthMultiplier;
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
            lifeSlider.maxValue = currentLife;
            lifeSlider.value = currentLife;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}