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
    [Header("Referencias y Config")]
    public TowerData config;
    public List<GameObject> towerImagen;
    public LineRenderer lineRenderer;
    [Header("Botones Interfaz")]
    public GameObject deleteTowerGameObject;
    public GameObject updateTowerGameObject;
    // Variables de Estado
    private float laserActiveTimer;    // Contador para los 5s
    private float laserRestTimer;      // Contador para los 2s
    private float currentRestDuration; // Cuánto dura el descanso (se reduce al mejorar)
    private bool isOverheated = false;
    private float damageAccrued = 0f;
    private GameObject projectilePrefab; // Se llena en el Start
    private float fireCooldown;          // Tiempo entre balas
    private float fireTimer = 0f;

    [HideInInspector] public float attackRadius;
    [HideInInspector] public int currentDamage;
    [HideInInspector] public int upgradeDamageStep;
    [HideInInspector] public float upgradeCooldownStep;
    [HideInInspector] public int totalGoldInvested = 0;
    [HideInInspector] public bool isBuilt = false;

    private Transform currentTarget;
    private SpriteRenderer spriteRenderer;
    private DeleteTower deletetower;
    private UpdateTower updatetower;
    private GameManager gameManager;
    private ConstructionMenu constructionMenu;
    public static GameObject gameObjectUpdateDeleteTower;
    public static Tower towerActiveInMenu;
    int circleSegments = 50;
    private void Awake() => towerActiveInMenu = null;
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
        constructionMenu = FindAnyObjectByType<ConstructionMenu>();

        if (config != null)
        {
            attackRadius = config.baseAttackRadius;
            upgradeDamageStep = config.damageUpgradeAmount;
            upgradeCooldownStep = config.cooldownUpgradeAmount;

            // CARGA SEGÚN EL TIPO DE DATA
            if (config is LaserTowerData laserData)
            {
                currentDamage = (int)laserData.damagePerSecond;
                laserActiveTimer = laserData.onTime;
                currentRestDuration = laserData.offTime;
            }
            else if (config is ProjectileTowerData projData)
            {
                currentDamage = projData.baseDamage;
                fireCooldown = projData.baseFireRate;
                projectilePrefab = projData.projectilePrefab;
            }
        }
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
        DrawRangeCircleInGame();

        if (EnemyTimeStopAbility.IsTimeStopped) return;

        if (config is LaserTowerData)
            HandleLaserAttack();
        else
            HandleProjectileAttack();

    }
    private void HandleLaserAttack()
    {
        LineRenderer lightningLase = null;
        Transform laserObj = transform.Find("towerInfernalLineRender");
        if (laserObj != null) lightningLase = laserObj.GetComponent<LineRenderer>();

        if (lightningLase == null) return;

        if (!isOverheated)
        {
            if (currentTarget != null)
            {
                lightningLase.enabled = true;
                lightningLase.positionCount = 2;
                lightningLase.SetPosition(0, transform.position);
                lightningLase.SetPosition(1, currentTarget.position);

                // Gasta batería
                laserActiveTimer -= Time.deltaTime;

                // Daño
                damageAccrued += currentDamage * Time.deltaTime;
                if (damageAccrued >= 1f)
                {
                    int dmg = Mathf.FloorToInt(damageAccrued);
                    currentTarget.GetComponent<Enemy>().TakeDamage(dmg);
                    damageAccrued -= dmg;
                }

                if (laserActiveTimer <= 0)
                {
                    isOverheated = true;
                    laserRestTimer = currentRestDuration;
                    lightningLase.enabled = false;
                }
            }
            else { lightningLase.enabled = false; }
        }
        else
        {
            // Descansando
            lightningLase.enabled = false;
            laserRestTimer -= Time.deltaTime;
            if (laserRestTimer <= 0)
            {
                isOverheated = false;
                laserActiveTimer = ((LaserTowerData)config).onTime;
            }
        }
    }

    private void HandleProjectileAttack()
    {
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
        constructionMenu.cancelFunction();
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
        float realRadius = attackRadius * GameManager.globalRadiusMultiplier;
        // 1. COMPROBAR EL OBJETIVO FIJADO
        if (currentTarget != null)
        {
            Enemy currentEnemyScript = currentTarget.GetComponent<Enemy>();
            if(currentEnemyScript == null)
            {
                currentTarget = null;
            }
            else
            {
                float distanceToCurrent = Vector2.Distance(transform.position, currentEnemyScript.transform.position);
                // Margen de tolerancia para cuando el jugador esta en el limite
                float dropRadius = realRadius + 0.5f;
                // Si está apagado, muerto, o se ha salido del círculo rojo...
                if (!currentTarget.gameObject.activeInHierarchy || currentEnemyScript.IsDead || distanceToCurrent > dropRadius)
                {
                    currentTarget = null; // ...lo soltamos.
                }
                else
                {
                    return;
                }
            }
            
        }
        // 2. BUSCAR UN NUEVO OBJETIVO
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float bestProgress = -Mathf.Infinity;
        GameObject bestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= realRadius)
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
        // Buscamos TODOS los botones que haya dentro del menú (el 'true' incluye los apagados)
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
        // 1. Si la torre ya está al NIVEL MÁXIMO, aquí sí ocultamos el botón por completo.
        if (updatetower.levelOfTower >= 2)
        {
            btnUpdate.gameObject.SetActive(false);
            return; // Salimos de la función, no hay nada más que hacer
        }
        // 2. Si NO es nivel máximo, el botón SIEMPRE debe estar visible en pantalla.
        btnUpdate.gameObject.SetActive(true);
        int indexToLook = !isBuilt ? 0 : updatetower.levelOfTower + 1;
        int costTower = config.upgradeCosts[indexToLook];

        TextMeshProUGUI textBoton = btnUpdate.GetComponentInChildren<TextMeshProUGUI>();

        // 3. Comprobamos la cartera del jugador
        if (GameManager.countMoney >= costTower * GameManager.globalCostMultiplier)
        {
            // TIENE DINERO: Botón clicable, coste en verde
            btnUpdate.interactable = true;
            textBoton.text = "MEJORAR\n(Coste: <color=#2ECC71>" + (costTower * GameManager.globalCostMultiplier) + "</color>)";

            btnUpdate.onClick.RemoveAllListeners();
            btnUpdate.onClick.AddListener(() => updatetower.onClickPlayer());
        }
        else
        {
            // NO TIENE DINERO: Botón bloqueado (gris), coste en rojo
            btnUpdate.interactable = false;
            textBoton.text = "MEJORAR\n(Coste: <color=#E74C3C>" + (costTower * GameManager.globalCostMultiplier) + "</color>)";

            // Quitamos los listeners por seguridad
            btnUpdate.onClick.RemoveAllListeners();
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
        int costTower = Mathf.RoundToInt(config.upgradeCosts[nextLevel] * GameManager.globalCostMultiplier);

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
            GameManager.sound(GameManager.soundError);
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
    /// Suma/resta estadísticas en Nivel 1 y 2.
    /// </summary>
    public void updateFireCooldownAndDamage()
    {
        if (updatetower.levelOfTower == 0) return;

        currentDamage += upgradeDamageStep;

        if (config is LaserTowerData)
        {
            // Mejora reduce el tiempo de espera de 2s
            currentRestDuration -= upgradeCooldownStep;
            currentRestDuration = Mathf.Max(currentRestDuration, 0.2f);
        }
        else
        {
            // Mejora reduce el tiempo entre balas
            fireCooldown -= upgradeCooldownStep;
            fireCooldown = Mathf.Max(fireCooldown, 0.1f);
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
                text.text = config.nameOfTower + " (Nivel " + (updatetower.levelOfTower + 1) + ")";
            }
            if (text.name == "currentDamageText")
            {
                if (updatetower.levelOfTower < 2)
                {
                    int increaseCurrentDamage = currentDamage + upgradeDamageStep;
                    text.text = "Daño: [" + currentDamage + "] -> <color=#2ECC71> [" + increaseCurrentDamage + "] </color>";
                }
                else
                {
                    text.text = "Daño: [" + currentDamage + "] (MÁXIMO)";
                }
            }
            if (text.name == "cadenceText")
            {
                cadenceFunction(text);
            }
        }
    }
    /// <summary>
    /// Dibuja un círculo usando un LineRenderer para que EL JUGADOR lo vea en la pestaña Game.
    /// Es ultra-óptimo: solo se dibuja si está seleccionada, y solo se borra 1 vez cuando se deselecciona.
    /// </summary>
    public void DrawRangeCircleInGame()
    {
        if (lineRenderer == null) return;

        // Si esta torre NO es la que está seleccionada en el menú...
        if (towerActiveInMenu != this)
        {
            // Comprobamos si el LineRenderer tiene puntos. Si ya tiene 0, ignoramos todo y salimos.
            // Preguntar "cuánto vale un int" es gratis para el procesador, pero modificar un componente de Unity no lo es.
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.positionCount = 0; // Lo borramos solo una vez
            }
            return;
        }

        // --- SI LLEGA HASTA AQUÍ, ES PORQUE SÍ ESTÁ SELECCIONADA ---

        // El radio real con multiplicadores
        float realRadius = attackRadius * GameManager.globalRadiusMultiplier;

        // Le decimos al LineRenderer cuántos puntos va a tener la línea
        lineRenderer.positionCount = circleSegments;
        lineRenderer.useWorldSpace = false; // Para que siga a la torre si se mueve
        lineRenderer.loop = true; // Cierra el círculo perfectamente

        // Matemáticas para dibujar un círculo punto por punto
        float angle = 0f;
        for (int i = 0; i < circleSegments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * realRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * realRadius;

            // PONEMOS LA Z EN -1 PARA QUE NO SE ESCONDA DETRÁS DEL CÉSPED
            lineRenderer.SetPosition(i, new Vector3(x, y, -1f));

            angle += (360f / circleSegments);
        }
    }
    public void cadenceFunction(TextMeshProUGUI text)
    {
        float currentBaseCooldown = config is LaserTowerData ? currentRestDuration : fireCooldown;
        if (updatetower.levelOfTower < 2)
        {
            // 1. Recarga REAL actual (Base * Cartas)
            float realCurrentCooldown = currentBaseCooldown * GameManager.globalAttackSpeedMultiplier;

            // 2. Simulamos cuál será la Base si la mejoramos
            float baseNextCooldown = currentBaseCooldown - upgradeCooldownStep;
            baseNextCooldown = Mathf.Max(baseNextCooldown, 0.1f); // Seguridad para la base

            // 3. Recarga REAL futura (Nueva Base * Cartas)
            float realNextCooldown = baseNextCooldown * GameManager.globalAttackSpeedMultiplier;
            realNextCooldown = Mathf.Max(realNextCooldown, 0.1f); // Seguridad final

            text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] -> <color=#2ECC71>[" + realNextCooldown.ToString("F2") + "s] </color>";
        }
        else
        {
            // Si ya es nivel máximo, calculamos su recarga real actual para mostrarla
            float realCurrentCooldown = currentBaseCooldown * GameManager.globalAttackSpeedMultiplier;
            text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] (MÁXIMO)";
        }
    }
}