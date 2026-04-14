using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Gestiona la detección de objetivos mediante un radio configurable y dispara.
/// También controla la lógica de construcción al hacer clic sobre la casilla vacía.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Atributos de la torre.")]
    [Tooltip("Radio de detección de los enemigos.")]
    public float attackRadius = 3f;

    [Tooltip("Tiempo de recarga en segundos entre cada disparo.")]
    public float fireCooldown = 1f;

    [Header("Referencias.")]
    [Tooltip("El prefab del proyectil que se va a instanciar.")]
    public GameObject projectilePrefab;

    [Tooltip("GameObject que contiene el Sprite y BoxCollider2D base de la torre.")]
    public GameObject towerImage;

    [Tooltip("Referencia al GameManager principal de la escena.")]
    public GameManager gameManager;

    private Transform currentTarget;
    private float fireTimer = 0f;
    private bool isBuilt = false;

    /// <summary>
    /// Comprueba frame a frame si la torre está construida.
    /// De ser así, busca objetivos y gestiona el tiempo de recarga para disparar.
    /// </summary>
    private void Update()
    {
        if (!isBuilt) return;

        UpdateTarget();

        if (currentTarget == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireCooldown * GameManager.globalAttackSpeedMultiplier;
        }
    }

    /// <summary>
    /// Busca enemigos dentro del radio y elige al que más adelantado va en el camino.
    /// </summary>
    private void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float bestProgress = -Mathf.Infinity;
        GameObject bestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= (attackRadius * GameManager.globalRadiusMultiplier))
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();

                if (enemyScript != null)
                {
                    float progress = enemyScript.GetPathProgress();

                    if (progress > bestProgress)
                    {
                        bestProgress = progress;
                        bestEnemy = enemy;
                    }
                }
            }
        }

        if (bestEnemy != null)
        {
            currentTarget = bestEnemy.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// Instancia un proyectil en la posición actual de la torre y le asigna el objetivo fijado.
    /// </summary>
    private void Shoot()
    {
        Vector3 startPos = transform.position;
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
        }
    }

    /// <summary>
    /// Se dispara automáticamente al hacer clic izquierdo sobre el objeto. 
    /// Construye la torre, actualiza las colisiones y desactiva el candado lógico.
    /// Suma uno al contador de torres activas
    /// </summary>
    private void OnMouseDown()
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        if (spriteRenderer.sprite.name == "Square" && GameManager.countMoney >= 100)
        {
            spriteRenderer.sprite = towerImage.GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D boxCollider2D = towerImage.GetComponent<BoxCollider2D>();
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            GameManager.countMoney -= (100 * GameManager.globalCostMultiplier).ConvertTo<int>();
        }
        else if (spriteRenderer.sprite.name != "Square")
        {
            StartCoroutine(gameManager.messageError("Lugar ya ocupado"));
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Dibuja el radio de ataque en color rojo en la vista de escena (Scene) 
    /// cuando la torre está seleccionada para ayudar visualmente en el diseño del nivel.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}