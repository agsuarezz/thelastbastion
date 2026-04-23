using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
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
    [Tooltip("EL prefab del proyectile que se va a instanciar.")]
    public GameObject projectilePrefab;

    [Tooltip("GameObject que contiene el Sprite y BoxCollider2D base de la torre.")]
    public List<GameObject> towerImagen;
    [Tooltip("Referencia al GameManager principal de la escena.")]
    GameManager gameManager;
    // ==========================================
    // ESTADO INTERNO DE LA TORRE
    // ==========================================

    /// <summary>
    /// El enemigo actual al que la torre está apuntando y disparando.
    /// </summary>
    private Transform currentTarget;
    /// <summary>
    /// Temporizador interno que cuenta el tiempo restante (en segundos) hasta el próximo disparo.
    /// </summary>
    private float fireTimer = 0f;
    /// <summary>
    /// Indica si la torre ya ha sido comprada y construida en esta casilla.
    /// </summary>
    [HideInInspector] public bool isBuilt = false;
    // ==========================================
    // INTERFAZ DE USUARIO Y COMPONENTES HIJOS
    // ==========================================

    /// <summary>
    /// Referencia al componente visual principal de la torre (su dibujo).
    /// </summary>
    SpriteRenderer spriteRenderer;
    /// <summary>
    /// Referencia al script hijo encargado de detectar si el jugador quiere vender/borrar esta torre.
    /// </summary>
    DeleteTower deletetower;
    /// <summary>
    /// Referencia al script hijo encargado de detectar si el jugador quiere mejorar (upgrade) esta torre.
    /// </summary>
    UpdateTower updatetower;
    /// <summary>
    /// El GameObject visual  que contiene el script de borrado y el sprite.
    /// </summary>
    public GameObject deleteTowerGameObject;
    /// <summary>
    /// El GameObject visualque contiene el script de mejora y el sprite.
    /// </summary>
    public GameObject updateTowerGameObject;
    [Tooltip("El daño actual de ESTA torre específica.")]
    public int currentDamage;

    ConstructionMenu constructionMenu;
    /// <summary>
    /// Inicializa las referencias principales de la torre (SpriteRenderer y los scripts de los botones hijos)
    /// y se asegura de que el menú emergente de selección empiece oculto al iniciar la partida.
    /// </summary>
    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        deletetower = this.GetComponentInChildren<DeleteTower>(true);
        updatetower = this.GetComponentInChildren<UpdateTower>(true);
        gameManager = FindAnyObjectByType<GameManager>();
        constructionMenu = FindAnyObjectByType<ConstructionMenu>();
        setInitialTower();
    }
    /// <summary>
    /// Se ejecuta frame a frame y controla el ciclo de vida de la torre:
    /// 1. Comprueba si el jugador ha solicitado una mejora (cambiando sprites y niveles).
    /// 2. Comprueba si el jugador ha vendido la torre (restaurando la casilla y devolviendo oro).
    /// 3. Si la torre está construida y activa, busca enemigos y gestiona el temporizador para disparar.
    /// </summary>
    private void Update()
    {
        if(updatetower && updatetower.needUpdateTower && updatetower.typeOfTower != -1)
        {
            if (updatetower.typeOfTower == 0)
            {
                if (updatetower.levelOfTower == 1)
                {
                    towerMedian(towerImagen[1].GetComponent<SpriteRenderer>().sprite, towerImagen[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if (updatetower.levelOfTower == 2)
                {
                    towerMedian(towerImagen[2].GetComponent<SpriteRenderer>().sprite, towerImagen[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }

            }
            if (updatetower.typeOfTower == 1)
            {
                if (updatetower.levelOfTower == 1)
                {
                    towerLight(towerImagen[1].GetComponent<SpriteRenderer>().sprite, towerImagen[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if (updatetower.levelOfTower == 2)
                {
                    towerLight(towerImagen[2].GetComponent<SpriteRenderer>().sprite, towerImagen[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }

            }
            if (updatetower.typeOfTower == 2)
            {
                if (updatetower.levelOfTower == 1)
                {
                    towerHeavy(towerImagen[1].GetComponent<SpriteRenderer>().sprite, towerImagen[1].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }
                else if (updatetower.levelOfTower == 2)
                {
                    towerHeavy(towerImagen[2].GetComponent<SpriteRenderer>().sprite, towerImagen[2].GetComponent<BoxCollider2D>());
                    updatetower.needUpdateTower = false;
                    return;
                }

            }

        }
        if (deletetower && deletetower.isDeleteTower)
        {
            GameManager.countMoney += 25;
            isBuilt = false;
            GameManager.countTower -= 1;
            Destroy(gameObject);
            return;
        }
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
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
            projectile.SetDamage(currentDamage);
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Mediana en el lugar seleccionado
    /// </summary>
    public void towerMedian(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (sprite == null)
            sprite = towerImagen[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
        int costTower = updatetower.costTower(0);
        if (GameManager.countMoney >= costTower)
        {
            updateExtensionsTower();

            setTypeTower(0);
            setCurrentDamage();
            Projectile projectilePrefabScript = projectilePrefab.GetComponent<Projectile>();
            updateFireCooldownAndDamage();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
            isBuilt = true;
            increaseCountTower();
            if (updatetower.levelOfTower >= 1)
                GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Ligera en el lugar seleccionado
    /// </summary>
    public void towerLight(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (sprite == null)
            sprite = towerImagen[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
        int costTower = updatetower.costTower(1);
        if (GameManager.countMoney >= costTower)
        {
            updateExtensionsTower();
            setTypeTower(1);
            setCurrentDamage();
            updateFireCooldownAndDamage();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
            isBuilt = true;
            increaseCountTower();
            if(updatetower.levelOfTower >= 1)
                GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
        }
    }

    /// <summary>
    /// Desactiva el menu de Selector de Torres y Añade a una Torre Pesada en el lugar seleccionado
    /// </summary>
    public void towerHeavy(Sprite sprite = null, BoxCollider2D boxCollider = null)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (sprite == null)
            sprite = towerImagen[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
        int costTower = updatetower.costTower(2);
        if (GameManager.countMoney >= costTower)
        {
            updateExtensionsTower();
            setTypeTower(2);
            setCurrentDamage();
            updateFireCooldownAndDamage();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
            isBuilt = true;
            increaseCountTower();
            if (updatetower.levelOfTower >= 1)
                GameManager.countMoney -= (costTower * GameManager.globalCostMultiplier).ConvertTo<int>();
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
    public void setCurrentDamage()
    {
        if (updatetower.levelOfTower == 0)
            if (updatetower.typeOfTower == 0)
                currentDamage = 20;
            else if (updatetower.typeOfTower == 1)
                currentDamage = 10;
            else
                currentDamage = 40;
    }
    public void setTypeTower(int type)
    {
        updatetower.typeOfTower = type;
    }
    public void updateExtensionsTower()
    {
        deleteTowerGameObject.SetActive(true);
        updateTowerGameObject.SetActive(true);
    }
    public void setCollisionsAndSprite(SpriteRenderer spriteRenderer, Sprite sprite, BoxCollider2D boxCollider)
    {
        spriteRenderer.sprite = sprite;
        this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider.size.x, boxCollider.size.y);

    }
     public void updateFireCooldownAndDamage()
    {
        if(updatetower.levelOfTower == 0)
        {
            switch (updatetower.typeOfTower)
            {
                case 1:
                    fireCooldown = 0.5f;
                    return;
                case 2:
                    fireCooldown = 2f;
                    return;
                default:
                    fireCooldown = 1f;
                    return;
            }
        }
        if (updatetower.levelOfTower > 0)
        {
            switch (updatetower.typeOfTower)
            {
                case 0:
                    fireCooldown += 0.5f;
                    currentDamage += 10;
                    return;
                case 1:
                    fireCooldown += 1f;
                    currentDamage += 5;
                    return;
                case 2:
                    fireCooldown += 0.25f;
                    currentDamage += 20;
                    return;
                default:
                    fireCooldown += 0.5f;
                    currentDamage += 10;
                    return;
            }
        }
    }
    public void increaseCountTower()
    {
        if(updatetower.levelOfTower == 0)
        {
            GameManager.countTower += 1;
        }
    }
    public void setInitialTower()
    {
        switch (constructionMenu.flagTypeTower)
        {
            case 0: towerMedian(towerImagen[0].GetComponent<SpriteRenderer>().sprite, towerImagen[0].GetComponent<BoxCollider2D>()); return;
            case 1: towerLight(towerImagen[0].GetComponent<SpriteRenderer>().sprite, towerImagen[0].GetComponent<BoxCollider2D>()); return;
            case 2: towerHeavy(towerImagen[0].GetComponent<SpriteRenderer>().sprite, towerImagen[0].GetComponent<BoxCollider2D>()); return;
            default: return;
        }
    }
}