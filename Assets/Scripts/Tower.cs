using System.Collections.Generic;
using TMPro;
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
    public static GameObject gameObjectUpdateDeleteTower;
    // Comprueba SI tiene el menu de Delete y Update activo
    public static Tower towerActiveInMenu;
    private void Awake()
    {
        towerActiveInMenu = null;
    }
    /// <summary>
    /// Inicializa las referencias de los componentes, busca el menú global en la escena 
    /// y autoconstruye la torre base basándose en la elección del jugador en el menú de construcción.
    /// </summary>
    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        deletetower = this.GetComponentInChildren<DeleteTower>(true);
        updatetower = this.GetComponentInChildren<UpdateTower>(true);
        gameManager = FindAnyObjectByType<GameManager>();
        gameObjectUpdateDeleteTower = GameObject.Find("gameObjectUpdateDeleteTower");
        if (gameObjectUpdateDeleteTower == null)
            Debug.LogWarning("No se ha podido dectectar ");
        constructionMenu = FindAnyObjectByType<ConstructionMenu>();
        SetTower(null, null, constructionMenu.flagTypeTower);
    }
    /// <summary>
    /// Se ejecuta frame a frame y controla el ciclo de vida de la torre:
    /// 1. Comprueba si el jugador ha solicitado una mejora (cambiando sprites y niveles).
    /// 2. Comprueba si el jugador ha vendido la torre (restaurando la casilla y devolviendo oro).
    /// 3. Si la torre está construida y activa, busca enemigos y gestiona el temporizador para disparar.
    /// </summary>
    private void Update()
    {
        if (towerActiveInMenu == this)
        {
            refreshButtonUpdate();
        }
        if (updatetower && updatetower.needUpdateTower && updatetower.typeOfTower != -1)
        {
            if (updatetower.levelOfTower == 1)
            {
                SetTower(towerImagen[1].GetComponent<SpriteRenderer>().sprite, towerImagen[1].GetComponent<BoxCollider2D>(), updatetower.typeOfTower);
                updatetower.needUpdateTower = false;
                return;
            }
            else if (updatetower.levelOfTower == 2)
            {
                SetTower(towerImagen[2].GetComponent<SpriteRenderer>().sprite, towerImagen[2].GetComponent<BoxCollider2D>(), updatetower.typeOfTower);
                updatetower.needUpdateTower = false;
                return;
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
    /// Se ejecuta al hacer clic sobre la torre. Muestra el menú global de la interfaz 
    /// y "formatea" los botones para que apunten a las funciones de ESTA torre específica.
    /// </summary>
    public void OnMouseDown()
    {
        towerActiveInMenu = this;
        if (GameManager.currentState != GameState.Playing) return;
        setGameObjectUpDeleStatus(true);

        // 1. Preparamos las variables
        Button btnDelete = null;
        Button btnCancel = null;

        // 2. Buscamos TODOS los botones que haya dentro del menú (el 'true' incluye los apagados)
        Button[] todosLosBotones = gameObjectUpdateDeleteTower.GetComponentsInChildren<Button>(true);

        // 3. Los asignamos por su nombre exacto
        foreach (Button btn in todosLosBotones)
        {
            if (btn.gameObject.name == "ButtonDeleteTower") btnDelete = btn;
            if (btn.gameObject.name == "ButtonCancelTower") btnCancel = btn;
        }

        // --- Comprobación de seguridad por si te has equivocado en algún nombre en Unity ---
        if (btnDelete == null || btnCancel == null)
        {
            Debug.LogError("¡Cuidado Jefe! No encuentro algún botón. Revisa que se llamen EXACTAMENTE ButtonDeleteTower y ButtonCancelTower en la jerarquía.");
            return;
        }

        // 5. ¡LA MAGIA! Limpiamos la memoria de los botones para que olviden otras torres
        btnDelete.onClick.RemoveAllListeners();
        btnCancel.onClick.RemoveAllListeners();

        // 6. Añadimos las funciones de ESTA torre en concreto
        btnDelete.onClick.AddListener(() => deletetower.onClickPlayer());
        btnCancel.onClick.AddListener(() => setGameObjectUpDeleStatus(false));
    }
    /// <summary>
    /// Enciende o apaga todos los elementos hijos del menú global de la torre en la interfaz (UI).
    /// </summary>
    public static void setGameObjectUpDeleStatus(bool status)
    {
        foreach (Transform hijo in gameObjectUpdateDeleteTower.transform)
        {
            hijo.gameObject.SetActive(status);
        }
        if(!status)
            towerActiveInMenu = null;
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
    public void refreshButtonUpdate()
    {
        Button btnUpdate = null;
        // 2. Buscamos TODOS los botones que haya dentro del menú (el 'true' incluye los apagados)
        Button[] todosLosBotones = gameObjectUpdateDeleteTower.GetComponentsInChildren<Button>(true);
        foreach (Button btn in todosLosBotones)
        {
            if (btn.gameObject.name == "ButtonUpdateTower") btnUpdate = btn;
        }
        if (btnUpdate == null)
        {
            Debug.LogError("¡Cuidado Jefe! No encuentro algún botón. Revisa que se llamen EXACTAMENTE ButtonUpdateTower en la jerarquía.");
            return;
        }
        if (updatetower.levelOfTower < 2 && GameManager.countMoney >= updatetower.costTower(updatetower.typeOfTower))
        {
            btnUpdate.gameObject.SetActive(true);
            btnUpdate.GetComponentInChildren<TextMeshProUGUI>().text = "MEJORAR\n" + "(Coste: " + updatetower.costTower(updatetower.typeOfTower) + ")";
            btnUpdate.onClick.RemoveAllListeners();
            btnUpdate.onClick.AddListener(() => updatetower.onClickPlayer());
        }
        else
            btnUpdate.gameObject.SetActive(false);
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
    /// Configura la torre recién comprada o mejorada. Valida si hay dinero suficiente, 
    /// actualiza sus estadísticas (sprite, colisiones, daño, cadencia) y cobra el coste al jugador.
    /// </summary>
    public void SetTower(Sprite sprite = null, BoxCollider2D boxCollider = null, int type = 0)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        if (sprite == null)
            sprite = towerImagen[0].GetComponent<SpriteRenderer>().sprite;
        if (boxCollider == null)
            boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
        int costTower = updatetower.costTower(type);
        if (GameManager.countMoney >= costTower)
        {
            updateExtensionsTower();

            setTypeTower(type);
            setCurrentDamage();
            Projectile projectilePrefabScript = projectilePrefab.GetComponent<Projectile>();
            updateFireCooldownAndDamage();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
            isBuilt = true;
            increaseCountTower();
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
    /// <summary>
    /// Establece el daño inicial (Nivel 0) de la torre dependiendo de su tipo: 
    /// Mediana (0), Ligera (1) o Pesada (2).
    /// </summary>
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
    /// <summary>
    /// Asigna el tipo de torre al script hijo encargado de gestionar las futuras mejoras.
    /// </summary>
    public void setTypeTower(int type)
    {
        updatetower.typeOfTower = type;
    }
    /// <summary>
    /// Activa los GameObjects internos de la torre que contienen los scripts de actualización y borrado.
    /// </summary>
    public void updateExtensionsTower()
    {
        deleteTowerGameObject.SetActive(true);
        updateTowerGameObject.SetActive(true);
    }
    /// <summary>
    /// Actualiza la apariencia física de la torre, aplicándole un nuevo sprite y ajustando 
    /// su caja de colisión (BoxCollider2D) al tamaño exacto de esa nueva imagen.
    /// </summary>
    public void setCollisionsAndSprite(SpriteRenderer spriteRenderer, Sprite sprite, BoxCollider2D boxCollider)
    {
        spriteRenderer.sprite = sprite;
        this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider.size.x, boxCollider.size.y);

    }
    /// <summary>
    /// Recalcula y aplica las estadísticas de combate (cadencia de tiro y daño extra) 
    /// basándose en el tipo de la torre y su nivel actual de mejora.
    /// </summary>
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
    /// <summary>
    /// Suma 1 al contador global de torres del GameManager, asegurándose de hacerlo 
    /// solo si es una torre recién construida (Nivel 0) y no una mejora.
    /// </summary>
    public void increaseCountTower()
    {
        if(updatetower.levelOfTower == 0)
        {
            GameManager.countTower += 1;
        }
    }
}