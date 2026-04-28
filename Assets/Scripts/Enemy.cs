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

    private Vector3[] pathWaypoints;
    private int currentWaypointIndex = 0;

    private bool isDead = false;
    public bool IsDead => isDead;

    private bool showLifeBar = true;

    private bool isAttackingCastle = false;
    private float attackTimer = 0f;

    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private castleScript targetCastle;
    private RangedEnemyAttack rangedAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        rangedAttack = GetComponent<RangedEnemyAttack>();
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
            if (rangedAttack != null)
            {
                rangedAttack.Tick();
            } else
            {
                AttackCastle();
            }
            
            return;
        }

        MoveAlongPath();
    }

    public void SetPath(Vector3[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
        isAttackingCastle = false;
        attackTimer = 0f;

        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
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
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }

        if (pathWaypoints == null || pathWaypoints.Length == 0) return;

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

        Vector3 targetWaypoint = pathWaypoints[currentWaypointIndex];

        Vector3 direction = targetWaypoint - transform.position;
        transform.Translate(direction.normalized * currentSpeed * GameManager.globalSpeedMultiplier * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, targetWaypoint) <= 0.1f)
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

        if (rangedAttack != null)
        {
            rangedAttack.ResetTimer();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
        }
    }

    private void AttackCastle()
    {
        if (targetCastle == null)
        {
            targetCastle = FindObjectOfType<castleScript>();
            if (targetCastle == null) return;
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
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

        if (isAttackingCastle)
            return pathWaypoints.Length + 1f;

        if (currentWaypointIndex >= pathWaypoints.Length)
            return pathWaypoints.Length;

        float progress = currentWaypointIndex;

        Vector3 targetWaypoint = pathWaypoints[currentWaypointIndex];

        if (currentWaypointIndex > 0)
        {
            Vector3 previousWaypoint = pathWaypoints[currentWaypointIndex - 1];
            float segmentLength = Vector2.Distance(previousWaypoint, targetWaypoint);
            float distanceFromPrevious = Vector2.Distance(previousWaypoint, transform.position);

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
            animator.SetBool("IsAttacking", false);
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
            animator.SetBool("IsAttacking", false);
        }

        if (rangedAttack != null)
        {
            rangedAttack.ResetTimer();
        }
    }
}