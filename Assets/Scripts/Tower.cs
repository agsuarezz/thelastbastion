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

        Debug.Log("La torre está buscando...");
        UpdateTarget();

        if (currentTarget == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireCooldown;
        }
    }

    /// <summary>
    /// Busca a todos los enemigos en la escena, calcula la distancia hasta ellos 
    /// y fija como objetivo al más cercano dentro del radio de ataque.
    /// </summary>
    private void UpdateTarget()
    {
        Debug.Log("Me estoy ejecutando jijiji");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log(gameObject.name + " ve " + enemies.Length + " enemigos en el mapa.");

        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            Debug.Log("El enemigo más cercano está a una distancia de: " + shortestDistance + ". Mi radio es: " + attackRadius);
        }

        if (nearestEnemy != null && shortestDistance <= attackRadius)
        {
            currentTarget = nearestEnemy.transform;
            Debug.Log("¡OBJETIVO FIJADO! Preparando disparo...");
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
    /// </summary>
    private void OnMouseDown()
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        if (spriteRenderer.sprite.name == "Square")
        {
            spriteRenderer.sprite = towerImage.GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D boxCollider2D = towerImage.GetComponent<BoxCollider2D>();
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
        }
        else
            StartCoroutine(gameManager.messageError("Lugar ya ocupado"));
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