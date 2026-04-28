using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Datos")]
    public EnemyData enemyData;

    [Header("UI")]
    public Slider lifeSlider;

    public float currentLife;
    public float currentSpeed;

    private Vector3[] pathWaypoints;
    private int currentWaypointIndex;

    private bool isDead;
    public bool IsDead => isDead;

    private bool showLifeBar = true;
    private bool isAttackingCastle;

    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private castleScript targetCastle;

    private Transform graphics;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        graphics = transform;
    }

    private void Start()
    {
        FindCastle();
    }

    private void Update()
    {
        if (isDead) return;

        if (!isAttackingCastle)
        {
            MoveAlongPath();
            
        }
    }

    public void SetPath(Vector3[] routeWaypoints)
    {
        pathWaypoints = routeWaypoints;
        currentWaypointIndex = 0;
        isAttackingCastle = false;

        SetAttackingAnimation(false);
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
        SetAttackingAnimation(false);

        if (pathWaypoints == null || pathWaypoints.Length == 0) return;

        if (IsCastleInAttackRange())
        {
            StartAttackingCastle();
            return;
        }

        if (currentWaypointIndex >= pathWaypoints.Length)
        {
            StartAttackingCastle();
            return;
        }

        Vector3 targetWaypoint = pathWaypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint - transform.position;
        FlipSprite(direction.x);

        transform.Translate(
            direction.normalized * currentSpeed * GameManager.globalSpeedMultiplier * Time.deltaTime,
            Space.World
        );

        if (Vector2.Distance(transform.position, targetWaypoint) <= 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    private bool IsCastleInAttackRange()
    {
        if (targetCastle == null)
        {
            FindCastle();
        }

        if (targetCastle == null || targetCastle.castleCollider == null) return false;

        Vector2 closestPoint = targetCastle.castleCollider.ClosestPoint(transform.position);
        float distance = Vector2.Distance(transform.position, closestPoint);

        return distance <= enemyData.attackRange;
    }

    private void StartAttackingCastle()
    {
        if (isAttackingCastle) return;

        isAttackingCastle = true;
        currentSpeed = 0f;

        StopMovement();
        SetAttackingAnimation(true);
    }

   
    public void ApplyCastleAttackDamage()
    {
        if (isDead) return;
        if (!isAttackingCastle) return;

        if (targetCastle == null)
        {
            FindCastle();
        }

        if (targetCastle == null) return;

        int finalDamage = Mathf.RoundToInt(enemyData.damage * GameManager.globalEnemyDamageMultiplier);
        targetCastle.TakeDamage(finalDamage);

       
    }

    


    private void SetAttackingAnimation(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", value);
        }
    }

    private void StopMovement()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void FindCastle()
    {
        targetCastle = FindObjectOfType<castleScript>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentLife -= damageAmount * GameManager.globalDamageTakenMultiplier;

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

        GameManager.enemiesDestroyed++;

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

    public void OnDeathAnimationFinished()
    {
        DestroyEnemy();
    }

    private void OnEnable()
    {
        isDead = false;
        isAttackingCastle = false;
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
    }

    private void FlipSprite(float directionX)
    {
        if (directionX == 0) return;

        Vector3 scale = graphics.localScale;

        if (directionX > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        graphics.localScale = scale;
    }
}