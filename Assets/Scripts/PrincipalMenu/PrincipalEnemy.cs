using UnityEngine;
using UnityEngine.UI;

public class PrincipalEnemy : MonoBehaviour
{
    [Header("Datos")]
    public EnemyData enemyData;

    [Header("UI")]
    public Slider lifeSlider;

    private float currentSpeed;

    private Vector3[] pathPositions;
    private int currentPointIndex = 0;

    private bool isActive = false;

    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        currentPointIndex = 0;
        isActive = true;

        if (enemyData != null)
        {
            currentSpeed = enemyData.speed;
        }
        else
        {
            currentSpeed = 1.5f;
        }

        if (lifeSlider != null)
        {
            lifeSlider.gameObject.SetActive(false);
        }

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

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        MoveAlongPath();
    }

    public void SetPath(Vector3[] routePositions)
    {
        pathPositions = routePositions;
        currentPointIndex = 0;
    }

    private void MoveAlongPath()
    {
        if (pathPositions == null || pathPositions.Length == 0)
            return;

        if (currentPointIndex >= pathPositions.Length)
        {
            DisableEnemy();
            return;
        }

        Vector3 targetPosition = pathPositions[currentPointIndex];
        Vector3 direction = targetPosition - transform.position;

        transform.Translate(direction.normalized * currentSpeed * Time.deltaTime, Space.World);

        if (Vector2.Distance(transform.position, targetPosition) <= 0.1f)
        {
            currentPointIndex++;
        }
    }

    public void DisableEnemy()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}