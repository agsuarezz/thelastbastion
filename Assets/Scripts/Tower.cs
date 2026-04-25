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
    // ==========================================
    // DICCIONARIO DE MEJORAS (0: Mediana, 1: Ligera, 2: Pesada)
    // ==========================================
    // Cuánto daño gana cada torre al subir de nivel:
    public static readonly int[] damageUpgradeAmount = { 15, 8, 40 };

    // Cuánto tiempo de recarga se reduce al subir de nivel:
    public static readonly float[] cooldownUpgradeAmount = { 0.2f, 0.05f, 0.25f };

    public static readonly Dictionary<int, int[]> upgradeCosts = new Dictionary<int, int[]>()
    {
        { 0, new int[] { 40, 90, 150 } }, // Media: [Compra, Niv1, Niv2]
        { 1, new int[] { 50, 75, 130 } }, // Ligera: [Compra, Niv1, Niv2]
        { 2, new int[] { 70, 120, 200 } }  // Pesada: [Compra, Niv1, Niv2]
    };
    [HideInInspector] public int totalGoldInvested = 0;
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
        if (updatetower && updatetower.needUpdateTower && updatetower.typeOfTower != -1)
        {
            int nextLevel = updatetower.levelOfTower + 1;
            // Cogemos el sprite del nivel al que acabamos de subir (1 o 2)
            Sprite proximoSprite = towerImagen[nextLevel].GetComponent<SpriteRenderer>().sprite;
            BoxCollider2D proximoCol = towerImagen[nextLevel].GetComponent<BoxCollider2D>();

            SetTower(proximoSprite, proximoCol, updatetower.typeOfTower);

            updatetower.needUpdateTower = false; // Reset de la bandera
            return; // Salimos del frame para evitar conflictos

        }
        if (deletetower && deletetower.isDeleteTower)
        {
            int goldRecovered = Mathf.RoundToInt(totalGoldInvested * 0.75f);
            GameManager.countMoney += goldRecovered;
            isBuilt = false;
            GameManager.countTower -= 1;
            Destroy(gameObject);
            return;
        }
        if (towerActiveInMenu == this)
        {
            refreshButtonUpdate();
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
        if (GameManager.currentState != GameState.Playing) return;

        towerActiveInMenu = this;
        setGameObjectUpDeleStatus(true);
        assignInformationImage();
        assignInformationText();

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
        int goldRecovered = Mathf.RoundToInt(totalGoldInvested * 0.75f);
        // --- Asignamos cuanto dinero recuperaria si vende ---
        btnDelete.GetComponentInChildren<TextMeshProUGUI>().text = "VENDER (Recuperas: " + goldRecovered + ")";
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
        int indexToLook = !isBuilt ? 0 : updatetower.levelOfTower + 1;
        int costTower = upgradeCosts[updatetower.typeOfTower][indexToLook];
        if (updatetower.levelOfTower < 2 && GameManager.countMoney >= costTower)
        {
            btnUpdate.gameObject.SetActive(true);
            btnUpdate.GetComponentInChildren<TextMeshProUGUI>().text = "MEJORAR\n" + "(Coste: " + costTower + ")";
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
    /// Configura la torre recién comprada o mejorada.
    /// </summary>
    public void SetTower(Sprite sprite = null, BoxCollider2D boxCollider = null, int type = 0)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        // 1. Calculamos el nivel al que vamos y el coste
        int nextLevel = !isBuilt ? 0 : updatetower.levelOfTower + 1;
        int costTower = Mathf.RoundToInt(upgradeCosts[type][nextLevel] * GameManager.globalCostMultiplier);

        if (GameManager.countMoney >= costTower)
        {
            // 2. Cobramos y preparamos las variables
            setCountMoneyTotalGoldInvested(costTower);
            updateExtensionsTower();
            setTypeTower(type);

            // 3. ¡LA CLAVE! Separamos Construir vs Mejorar para el orden de estadísticas
            if (!isBuilt)
            {
                // ES NUEVA (Nivel 0)
                isBuilt = true;
                updatetower.levelOfTower = 0;
                setCurrentDamage(); // Ponemos el daño base inicial
                updateFireCooldownAndDamage(); // Ponemos la recarga base
                increaseCountTower();
            }
            else
            {
                // ES UNA MEJORA
                updatetower.levelOfTower++; // SUBIMOS EL NIVEL PRIMERO
                updateFireCooldownAndDamage(); // Como el nivel ya subió, ahora sí sumará el daño extra
            }

            // 4. Aplicamos los visuales
            if (sprite == null) sprite = towerImagen[0].GetComponent<SpriteRenderer>().sprite;
            if (boxCollider == null) boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
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
    /// Establece el daño inicial (Nivel 0) de la torre.
    /// </summary>
    public void setCurrentDamage()
    {
        // Ya no hace falta comprobar el nivel aquí, SetTower solo llama a esto en Nivel 0.
        if (updatetower.typeOfTower == 0)
            currentDamage = 25; // Mediana
        else if (updatetower.typeOfTower == 1)
            currentDamage = 12; // Ligera
        else
            currentDamage = 60; // Pesada
    }
    /// <summary>
    /// Asigna el tipo de torre al script hijo encargado de gestionar las futuras mejoras.
    /// </summary>
    public void setTypeTower(int type)
    {
        updatetower.typeOfTower = type;
    }

    public void setCountMoneyTotalGoldInvested(int finalCost)
    {
        // 2. Se lo restamos al dinero del jugador
        GameManager.countMoney -= finalCost;

        // 3. ¡Lo guardamos en la hucha de la torre para su futura venta!
        totalGoldInvested += finalCost;
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
    /// Aplica las estadísticas base en Nivel 0, o suma/resta estadísticas en Nivel 1 y 2.
    /// </summary>
    public void updateFireCooldownAndDamage()
    {
        if (updatetower.levelOfTower == 0)
        {
            switch (updatetower.typeOfTower)
            {
                case 0: fireCooldown = 1.0f; return; // Mediana
                case 1: fireCooldown = 0.4f; return; // Ligera
                case 2: fireCooldown = 2.0f; return; // Pesada
                default: fireCooldown = 1.0f; return;
            }
        }

        // Si el código llega hasta aquí, es porque el nivel es > 0 (es una mejora)
        currentDamage += damageUpgradeAmount[updatetower.typeOfTower];
        fireCooldown -= cooldownUpgradeAmount[updatetower.typeOfTower];
        fireCooldown = Mathf.Max(fireCooldown, 0.1f); // Límite de seguridad
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
    /// <summary>
    /// Actualiza la imagen (sprite) mostrada en el panel de información, visualizando 
    /// cómo se verá la torre si el jugador decide mejorarla al siguiente nivel.
    /// Si ya está al máximo, muestra la imagen del nivel actual.
    /// </summary>
    public void assignInformationImage()
    {
        Image[] imageList = gameObjectUpdateDeleteTower.GetComponentsInChildren<Image>();
        foreach (Image image in imageList)
        {
            if(image.gameObject.name == "towerImageUpgrade")
            {
                if(updatetower.levelOfTower < 2)
                {
                    image.sprite = towerImagen[updatetower.levelOfTower + 1].GetComponent<SpriteRenderer>().sprite;
                    return;
                }
                image.sprite = towerImagen[updatetower.levelOfTower].GetComponent<SpriteRenderer>().sprite;
                return;
            }
        }
    }
    /// <summary>
    /// Actualiza los textos del panel de información (Nombre, Daño y Recarga).
    /// Calcula las estadísticas futuras de la torre y las muestra con etiquetas 
    /// de color verde para destacar que es una mejora positiva.
    /// </summary>
    public void assignInformationText()
    {
        TextMeshProUGUI[] textList = gameObjectUpdateDeleteTower.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in textList)
        {
            if (text.name == "typeLevelText")
            {
                text.text = nameTypeOfTower() + " (Niv." + updatetower.levelOfTower + ")";
            }
            if (text.name == "currentDamageText")
            {
                if (updatetower.levelOfTower < 2)
                {
                    int increaseCurrentDamage = currentDamage + damageUpgradeAmount[updatetower.typeOfTower];
                    text.text = "Daño: [" + currentDamage + "] -> <color=#2ECC71> [" + increaseCurrentDamage + "] </color>";
                }
                else
                {
                    text.text = "Daño: [" + currentDamage + "] (MÁXIMO)";
                }
            }
            if (text.name == "cadenceText")
            {
                if (updatetower.levelOfTower < 2)
                {
                    // 1. Recarga REAL actual (Base * Cartas)
                    float realCurrentCooldown = fireCooldown * GameManager.globalAttackSpeedMultiplier;

                    // 2. Simulamos cuál será la Base si la mejoramos
                    float baseNextCooldown = fireCooldown - cooldownUpgradeAmount[updatetower.typeOfTower];
                    baseNextCooldown = Mathf.Max(baseNextCooldown, 0.1f); // Seguridad para la base

                    // 3. Recarga REAL futura (Nueva Base * Cartas)
                    float realNextCooldown = baseNextCooldown * GameManager.globalAttackSpeedMultiplier;
                    realNextCooldown = Mathf.Max(realNextCooldown, 0.1f); // Seguridad final

                    text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] -> <color=#2ECC71>[" + realNextCooldown.ToString("F2") + "s] </color>";
                }
                else
                {
                    // Si ya es nivel máximo, calculamos su recarga real actual para mostrarla
                    float realCurrentCooldown = fireCooldown * GameManager.globalAttackSpeedMultiplier;
                    text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] (MÁXIMO)";
                }
            }
        }
    }
    public string nameTypeOfTower()
    {
        switch (updatetower.typeOfTower)
        {
            case 0:
                return "Torre Media";
            case 1:
                return "Torre Ligera";
            case 2:
                return "Torre Pesada";
            default:
                return "Torre Media";
        }
    }
}