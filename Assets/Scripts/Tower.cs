using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la detección de objetivos mediante un radio configurable y dispara.
/// También controla la lógica de construcción al hacer clic sobre la casilla vacía.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Atributos de la torre (Algunos son privados).")]
    [Tooltip("Radio de detección de los enemigos.")]
    public float attackRadius = 1f;

    //Tiempo de recarga en segundos entre cada disparo.
    float fireCooldown = 1f;

    [Header("Referencias.")]
    [Tooltip("Los prefabs de los proyectiles que se va a instanciar.")]
    public GameObject projectilePrefab;
    public GameObject projectilePrefabTower2;
    public GameObject projectilePrefabTower3;

    [Tooltip("GameObject que contiene el Sprite y BoxCollider2D base de la torre.")]
    public GameObject towerImage;
    public GameObject towerImage2;
    public GameObject towerImage3;
    [Tooltip("Referencia al GameManager principal de la escena.")]
    public GameManager gameManager;

    private Transform currentTarget;
    private float fireTimer = 0f;
    private bool isBuilt = false;
    public GameObject menuTowerSelect;
    SpriteRenderer spriteRenderer;
    // Flag que indica el tipo de torre elegida por el usuario
    int typeTower = 0;
    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        menuTowerSelect.SetActive(false);
    }
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
                    if (enemyScript.IsDead) continue;

                    float progress = enemyScript.GetPathProgress();

                    if (progress > bestProgress)
                    {
                        bestProgress = progress;
                        bestEnemy = enemy;
                    }
                }
            }
        }

        currentTarget = bestEnemy != null ? bestEnemy.transform : null;
    }

    /// <summary>
    /// Instancia un proyectil en la posición actual de la torre y le asigna el objetivo fijado.
    /// </summary>
    private void Shoot()
    {
        Vector3 startPos = transform.position;
        if(typeTower == 1)
            projectilePrefab = projectilePrefabTower2;
        if(typeTower == 2)
            projectilePrefab = projectilePrefabTower3;
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
        }
    }

    /// <summary>
    /// Activa el menu de Selector de Torres y Añade a los botones su respectiva funcion que se encarga de todo
    /// </summary>
    private void OnMouseDown()
    {
        if(spriteRenderer.sprite.name == "Square")
        {
            menuTowerSelect.SetActive(true);
            // 1. Buscamos los botones
            Button btnMedian = GameObject.Find("ButtonTowerMedian").GetComponent<Button>();
            Button btnLight = GameObject.Find("ButtonTowerLight").GetComponent<Button>();
            Button btnHeavy = GameObject.Find("ButtonTowerHeavy").GetComponent<Button>();
            Button btnCancel = GameObject.Find("ButtonCancel").GetComponent<Button>();

            // 2. ¡LA MAGIA! Limpiamos la memoria de los botones para que olviden otras torres
            btnMedian.onClick.RemoveAllListeners();
            btnLight.onClick.RemoveAllListeners();
            btnHeavy.onClick.RemoveAllListeners();
            btnCancel.onClick.RemoveAllListeners();

            // 3. Añadimos las funciones de ESTA torre en concreto
            btnMedian.onClick.AddListener(towerMedian);
            btnLight.onClick.AddListener(towerLight);
            btnHeavy.onClick.AddListener(towerHeavy);
            btnCancel.onClick.AddListener(cancelFunction);
        }
        else if (spriteRenderer.sprite.name != "Square")
        {
            StartCoroutine(gameManager.messageError("Lugar ya ocupado"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres
    /// </summary>
    public void cancelFunction()
    {
        menuTowerSelect.SetActive(false);
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Mediana en el lugar seleccionado
    /// </summary>
    public void towerMedian()
    {
        
        int costTower = 50;
        if (GameManager.countMoney >= costTower)
        {
            spriteRenderer.sprite = towerImage.GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D boxCollider2D = towerImage.GetComponent<BoxCollider2D>();
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
            typeTower = 0;
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Ligera en el lugar seleccionado
    /// </summary>
    public void towerLight()
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        int costTower = 100;
        if (GameManager.countMoney >= costTower)
        {
            fireCooldown = 0.5f;
            spriteRenderer.sprite = towerImage2.GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D boxCollider2D = towerImage2.GetComponent<BoxCollider2D>();
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            typeTower = 1;
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Pesada en el lugar seleccionado
    /// </summary>
    public void towerHeavy()
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        int costTower = 100;
        if (GameManager.countMoney >= costTower)
        {
            fireCooldown = 2f;
            spriteRenderer.sprite = towerImage3.GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D boxCollider2D = towerImage3.GetComponent<BoxCollider2D>();
            this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            isBuilt = true;
            GameManager.countTower += 1;
            typeTower = 2;
            GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
            menuTowerSelect.SetActive(false);
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