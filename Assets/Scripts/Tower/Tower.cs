using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float upgradeDamageStep;
    [HideInInspector] public float upgradeCooldownStep;
    [HideInInspector] public int totalGoldInvested = 0;
    [HideInInspector] public bool isBuilt = false;

    // Bonificaciones locales por carta (suman a las globales)
    [HideInInspector] public float localBurnBonus = 0f;
    [HideInInspector] public float localPoisonBonus = 0f;
    [HideInInspector] public float localChainBonus = 0f;
    [HideInInspector] public float localSlowBonus = 0f;

    private bool destroyedByEnemy = false;
    [HideInInspector] public float currentIncreaseDamage;
    private List<Tower> buffedTowers = new List<Tower>();

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

    public ShortCutScript shortCuts;

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
            float treeIncreaseDamage = GameManager.metaProgression.upgradesTree[config.nameOfTower][0];
            float treeIncreaseRadius = GameManager.metaProgression.upgradesTree[config.nameOfTower][1];
            float treeIncreaseVelocity = GameManager.metaProgression.upgradesTree[config.nameOfTower][2];

            attackRadius = config.baseAttackRadius * treeIncreaseRadius;
            upgradeDamageStep = config.damageUpgradeAmount * treeIncreaseDamage;
            upgradeCooldownStep = config.cooldownUpgradeAmount / treeIncreaseVelocity;

            // CARGA SEGÚN EL TIPO DE DATA
            if (config is LaserTowerData laserData)
            {
                currentDamage = laserData.damagePerSecond * treeIncreaseDamage;
                laserActiveTimer = laserData.onTime / treeIncreaseVelocity;
                currentRestDuration = laserData.offTime;
            }
            else if (config is ProjectileTowerData projData)
            {
                currentDamage = projData.baseDamage * treeIncreaseDamage;
                fireCooldown = projData.baseFireRate / treeIncreaseVelocity;
                projectilePrefab = projData.projectilePrefab;
            }
            else if (config is SupportTowerData supportData)
            {
                currentIncreaseDamage = supportData.baseIncreaseDamage * treeIncreaseDamage;
                fireCooldown = supportData.baseFireRate / treeIncreaseVelocity;
            }

            // --- APLICACIÓN DE MEJORAS GLOBALES ACUMULADAS POR CARTAS ---
            string name = config.nameOfTower;
            CardManager.InitializeTowerDict(name);

            // Daño
            currentDamage *= CardManager.towerDamageMultipliers[name];
            if (config is SupportTowerData)
            {
                currentIncreaseDamage *= CardManager.towerDamageMultipliers[name];
            }
            upgradeDamageStep *= CardManager.towerDamageMultipliers[name]; // Para que al subirla de nivel respete el multiplicador

            // Radio
            attackRadius *= CardManager.towerRadiusMultipliers[name];

            // Velocidad de ataque
            ApplyLocalSpeedBonus(CardManager.towerSpeedMultipliers[name]);
            upgradeCooldownStep *= CardManager.towerSpeedMultipliers[name];

            // Efectos especiales de impacto
            localBurnBonus += CardManager.towerBurnBonus[name];
            localPoisonBonus += CardManager.towerPoisonBonus[name];
            localChainBonus += CardManager.towerChainBonus[name];
            localSlowBonus += CardManager.towerSlowBonus[name];
        }

        SetTower(null, null, constructionMenu.flagTypeTower);
    }

    private void Update()
    {
        if (updatetower && updatetower.needUpdateTower && updatetower.typeOfTower != -1)
        {
            int nextLevel = updatetower.levelOfTower + 1;
            SpriteRenderer nexSprite = towerImagen[nextLevel].GetComponent<SpriteRenderer>();
            BoxCollider2D nexCol = towerImagen[nextLevel].GetComponent<BoxCollider2D>();
            SetTower(nexSprite, nexCol, updatetower.typeOfTower);
            updatetower.needUpdateTower = false;
            return;
        }

        if (deletetower && deletetower.isDeleteTower && !destroyedByEnemy)
        {
            int goldRecovered = Mathf.RoundToInt(totalGoldInvested * 0.75f);
            GameManager.countMoney += goldRecovered;
            if (!GameManager.isLoadingGame)
            {
                GameManager.ShowFloatingMoney(goldRecovered, isGain: true);
                GameManager.sound(GameManager.soundMoney);
            }
            isBuilt = false;
            GameManager.countTower -= 1;
            if (config is SupportTowerData)
            {
                RemoveAllBuffs();
            }
            if (towerActiveInMenu == this)
            {
                setGameObjectUpDeleStatus(false);
            }
            Destroy(gameObject);
            return;
        }

        if (towerActiveInMenu == this)
        {
            refreshButtonUpdate();
            if (Input.GetKeyDown(shortCuts.keyToSellTower))
            {
                deletetower.onClickPlayer();
            }
        }

        if (!isBuilt) return;

        UpdateTarget();
        DrawRangeCircleInGame();

        if (EnemyTimeStopAbility.IsTimeStopped) return;

        if (config is LaserTowerData)
            HandleLaserAttack();
        else if (config is ProjectileTowerData)
            HandleProjectileAttack();
        else if (config is SupportTowerData)
            HandleIncreaseDamage();
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

                laserActiveTimer -= Time.deltaTime;
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
            lightningLase.enabled = false;
            laserRestTimer -= Time.deltaTime;
            if (laserRestTimer <= 0)
            {
                isOverheated = false;
                laserActiveTimer = ((LaserTowerData)config).onTime;
            }
        }
    }

    private void HandleIncreaseDamage()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireCooldown * GameManager.globalAttackSpeedMultiplier;
            float radius = attackRadius * GameManager.globalRadiusMultiplier;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("tower"))
                {
                    Tower nearbyTower = hit.GetComponent<Tower>();
                    if (nearbyTower != null && nearbyTower != this && nearbyTower.isBuilt && !buffedTowers.Contains(nearbyTower))
                    {
                        nearbyTower.currentDamage += currentIncreaseDamage;
                        buffedTowers.Add(nearbyTower);
                    }
                }
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

    public void OnMouseDown()
    {
        if (GameManager.currentState != GameState.Playing) return;
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        towerActiveInMenu = this;
        setGameObjectUpDeleStatus(true);
        constructionMenu.cancelFunction();
        assignInformationImage();
        assignInformationText();

        Button btnDelete = null;
        Button btnCancel = null;

        Button[] todosLosBotones = gameObjectUpdateDeleteTower.GetComponentsInChildren<Button>(true);

        foreach (Button btn in todosLosBotones)
        {
            if (btn.gameObject.name == "ButtonDeleteTower") btnDelete = btn;
            if (btn.gameObject.name == "ButtonCancelTower") btnCancel = btn;
        }

        if (btnDelete == null || btnCancel == null)
        {
            Debug.LogError("¡Cuidado Jefe! No encuentro algún botón. Revisa que se llamen EXACTAMENTE ButtonDeleteTower y ButtonCancelTower en la jerarquía.");
            return;
        }

        int goldRecovered = Mathf.RoundToInt(totalGoldInvested * 0.75f);
        btnDelete.GetComponentInChildren<TextMeshProUGUI>().text = "VENDER (Recuperas: " + goldRecovered + ")";

        btnDelete.onClick.RemoveAllListeners();
        btnCancel.onClick.RemoveAllListeners();

        btnDelete.onClick.AddListener(() => deletetower.onClickPlayer());
        btnCancel.onClick.AddListener(() => setGameObjectUpDeleStatus(false));
    }

    public static void setGameObjectUpDeleStatus(bool status)
    {
        foreach (Transform hijo in gameObjectUpdateDeleteTower.transform)
        {
            hijo.gameObject.SetActive(status);
        }
        if (!status)
            towerActiveInMenu = null;
    }

    private void UpdateTarget()
    {
        float realRadius = attackRadius * GameManager.globalRadiusMultiplier;

        if (currentTarget != null)
        {
            Enemy currentEnemyScript = currentTarget.GetComponent<Enemy>();
            if (currentEnemyScript == null)
            {
                currentTarget = null;
            }
            else
            {
                float distanceToCurrent = Vector2.Distance(transform.position, currentEnemyScript.transform.position);
                float dropRadius = realRadius + 0.5f;

                if (!currentTarget.gameObject.activeInHierarchy || currentEnemyScript.IsDead || distanceToCurrent > dropRadius)
                {
                    currentTarget = null;
                }
                else
                {
                    return;
                }
            }
        }

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
        Button[] todosLosBotones = gameObjectUpdateDeleteTower.GetComponentsInChildren<Button>(true);
        foreach (Button btn in todosLosBotones)
        {
            if (btn.gameObject.name == "ButtonUpdateTower") btnUpdate = btn;
        }
        if (btnUpdate == null) return;

        if (updatetower.levelOfTower >= 2)
        {
            btnUpdate.gameObject.SetActive(false);
            return;
        }

        btnUpdate.gameObject.SetActive(true);
        int indexToLook = !isBuilt ? 0 : updatetower.levelOfTower + 1;
        float costTower = config.upgradeCosts[indexToLook];

        TextMeshProUGUI textBoton = btnUpdate.GetComponentInChildren<TextMeshProUGUI>();

        if (GameManager.countMoney >= costTower * GameManager.globalCostMultiplier)
        {
            btnUpdate.interactable = true;
            textBoton.text = "MEJORAR\n(Coste: <color=#2ECC71>" + (costTower * GameManager.globalCostMultiplier) + "</color>)";
            btnUpdate.onClick.RemoveAllListeners();
            btnUpdate.onClick.AddListener(() => updatetower.onClickPlayer());
        }
        else
        {
            btnUpdate.interactable = false;
            textBoton.text = "MEJORAR\n(Coste: <color=#E74C3C>" + (costTower * GameManager.globalCostMultiplier) + "</color>)";
            btnUpdate.onClick.RemoveAllListeners();
        }
    }

    private void Shoot()
    {
        Vector3 startPos = transform.position;
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
            projectile.SetDamage(GetRealDamage(currentDamage));
            TryInjectBurnEffect(projectile);
            TryInjectPoisonEffect(projectile);
            TryInjectChainLightningEffect(projectile);
            TryInjectSlowEffect(projectile);
        }
    }

    private void TryInjectBurnEffect(Projectile projectile)
    {
        if (GameManager.globalBurnProbability <= 0f && localBurnBonus <= 0f) return;

        float effectiveBurnChance = Mathf.Min(GameManager.globalBurnProbability + localBurnBonus, 0.95f);
        bool burnTriggered = Random.value < effectiveBurnChance;
        if (!burnTriggered) return;

        const int burnDamagePerTick = 3;
        const float burnTickInterval = 0.5f;
        const int burnTotalTicks = 12;

        projectile.AddOnHitEffect(new BurnOnHitEffect(burnDamagePerTick, burnTickInterval, burnTotalTicks));
    }

    private void TryInjectPoisonEffect(Projectile projectile)
    {
        if ((GameManager.globalPoisonProbability + localPoisonBonus) <= 0f) return;

        float effectivePoisonChance = Mathf.Min(GameManager.globalPoisonProbability + localPoisonBonus, 0.95f);
        bool poisonTriggered = Random.value < effectivePoisonChance;
        if (!poisonTriggered) return;

        const int poisonBaseDamagePerTick = 10;
        const float poisonTickInterval = 0.8f;
        const int poisonTotalTicks = 10;
        const int poisonMaxStacks = 6;

        projectile.AddOnHitEffect(new PoisonOnHitEffect(poisonBaseDamagePerTick, poisonTickInterval, poisonTotalTicks, poisonMaxStacks));
    }

    private void TryInjectChainLightningEffect(Projectile projectile)
    {
        if ((GameManager.globalChainLightningChance + localChainBonus) <= 0f) return;

        float effectiveChainChance = Mathf.Min(GameManager.globalChainLightningChance + localChainBonus, 0.95f);
        bool triggered = Random.value < effectiveChainChance;
        if (!triggered) return;

        const float chainDamage = 15f;
        const float chainRadius = 3f;
        const int chainMaxJumps = 3;
        const float chainFalloff = 0.6f;

        projectile.AddOnHitEffect(new ChainLightningOnHitEffect(chainDamage, chainRadius, chainMaxJumps, chainFalloff));
    }

    private void TryInjectSlowEffect(Projectile projectile)
    {
        float effectiveSlowChance = Mathf.Min(GameManager.globalSlowChance + localSlowBonus, 0.95f);
        if (effectiveSlowChance <= 0f) return;

        // Los valores de ralentización se configuran desde CardManager (Inspector)
        CardManager cm = FindFirstObjectByType<CardManager>();
        float speedMult = cm != null ? cm.slowSpeedMultiplier : 0.5f;
        float duration  = cm != null ? cm.slowDuration        : 3f;

        projectile.AddOnHitEffect(new SlowOnHitEffect(effectiveSlowChance, speedMult, duration));
    }

    public void SetTower(SpriteRenderer sprite = null, BoxCollider2D boxCollider = null, int type = 0)
    {
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();

        int nextLevel = !isBuilt ? 0 : updatetower.levelOfTower + 1;
        int costTower = Mathf.RoundToInt(config.upgradeCosts[nextLevel] * GameManager.globalCostMultiplier);

        if (GameManager.countMoney >= costTower)
        {
            setCountMoneyTotalGoldInvested(costTower);
            updateExtensionsTower();
            setTypeTower(type);

            if (!isBuilt)
            {
                isBuilt = true;
                updatetower.levelOfTower = 0;
                updateFireCooldownAndDamage();
                increaseCountTower();
            }
            else
            {
                updatetower.levelOfTower++;
                updateFireCooldownAndDamage();
            }

            if (sprite == null) sprite = towerImagen[0].GetComponent<SpriteRenderer>();
            if (boxCollider == null) boxCollider = towerImagen[0].GetComponent<BoxCollider2D>();
            setCollisionsAndSprite(spriteRenderer, sprite, boxCollider);
        }
        else
        {
            StartCoroutine(gameManager.messageError("No hay dinero suficiente"));
            GameManager.sound(GameManager.soundError);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void setTypeTower(int type)
    {
        updatetower.typeOfTower = type;
    }

    public void setCountMoneyTotalGoldInvested(int finalCost)
    {
        GameManager.countMoney -= finalCost;

        if (!GameManager.isLoadingGame)
        {
            GameManager.ShowFloatingMoney(finalCost, isGain: false);
            GameManager.sound(GameManager.soundPay);
        }

        totalGoldInvested += finalCost;
    }

    public void updateExtensionsTower()
    {
        deleteTowerGameObject.SetActive(true);
        updateTowerGameObject.SetActive(true);
    }

    public void setCollisionsAndSprite(SpriteRenderer spriteRenderer, SpriteRenderer spriteRenderNew, BoxCollider2D boxCollider)
    {
        spriteRenderer.sprite = spriteRenderNew.sprite;
        spriteRenderer.color = spriteRenderNew.color;
        this.GetComponent<BoxCollider2D>().size = new Vector2(boxCollider.size.x, boxCollider.size.y);
    }

    public void updateFireCooldownAndDamage()
    {
        if (updatetower.levelOfTower == 0) return;

        RemoveAllBuffs();
        if (config is LaserTowerData || config is ProjectileTowerData)
        {
            currentDamage += upgradeDamageStep;
        }
        else if (config is SupportTowerData)
        {
            currentIncreaseDamage += upgradeDamageStep;
        }

        if (config is LaserTowerData)
        {
            currentRestDuration -= upgradeCooldownStep;
            currentRestDuration = Mathf.Max(currentRestDuration, 0.2f);
        }
        else
        {
            fireCooldown -= upgradeCooldownStep;
            fireCooldown = Mathf.Max(fireCooldown, 0.1f);
        }
    }

    public void ApplyLocalSpeedBonus(float multiplier)
    {
        fireCooldown *= multiplier;
        fireCooldown = Mathf.Max(fireCooldown, 0.1f);
        if (config is LaserTowerData)
        {
            currentRestDuration *= multiplier;
            currentRestDuration = Mathf.Max(currentRestDuration, 0.2f);
        }
    }

    public void increaseCountTower()
    {
        if (updatetower.levelOfTower == 0)
        {
            GameManager.countTower += 1;
        }
    }

    public void assignInformationImage()
    {
        Image[] imageList = gameObjectUpdateDeleteTower.GetComponentsInChildren<Image>();
        foreach (Image image in imageList)
        {
            if (image.gameObject.name == "towerImageUpgrade")
            {
                if (updatetower.levelOfTower < 2)
                {
                    image.sprite = towerImagen[updatetower.levelOfTower + 1].GetComponent<SpriteRenderer>().sprite;
                    return;
                }
                image.sprite = towerImagen[updatetower.levelOfTower].GetComponent<SpriteRenderer>().sprite;
                return;
            }
        }
    }

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
                damageFunction(text);
            }
            if (text.name == "cadenceText")
            {
                cadenceFunction(text);
            }
        }
    }

    public void DrawRangeCircleInGame()
    {
        if (lineRenderer == null) return;

        if (towerActiveInMenu != this)
        {
            if (lineRenderer.positionCount > 0)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        float realRadius = attackRadius * GameManager.globalRadiusMultiplier;
        lineRenderer.positionCount = circleSegments;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        float angle = 0f;
        for (int i = 0; i < circleSegments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * realRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * realRadius;

            lineRenderer.SetPosition(i, new Vector3(x, y, -1f));
            angle += (360f / circleSegments);
        }
    }

    public void damageFunction(TextMeshProUGUI text)
    {
        if (config is SupportTowerData)
        {
            if (updatetower.levelOfTower < 2)
            {
                float increaseSupportDamage = currentIncreaseDamage + upgradeDamageStep;
                text.text = "Daño Extra: [" + currentIncreaseDamage.ToString("F1") + "] -> <color=#2ECC71> [" + increaseSupportDamage.ToString("F1") + "] </color>";
            }
            else
            {
                text.text = "Daño Extra: [" + currentIncreaseDamage.ToString("F1") + "] (MÁXIMO)";
            }
        }
        else
        {
            float realCurrentDamage = GetRealDamage(currentDamage);
            if (updatetower.levelOfTower < 2)
            {
                float increaseCurrentDamage = GetRealDamage(currentDamage + upgradeDamageStep);
                text.text = "Daño: [" + realCurrentDamage.ToString("F1") + "] -> <color=#2ECC71> [" + increaseCurrentDamage.ToString("F1") + "] </color>";
            }
            else
            {
                text.text = "Daño: [" + realCurrentDamage.ToString("F1") + "] (MÁXIMO)";
            }
        }
    }

    public void cadenceFunction(TextMeshProUGUI text)
    {
        if (config is LaserTowerData || config is ProjectileTowerData)
        {
            float currentBaseCooldown = config is LaserTowerData ? currentRestDuration : fireCooldown;
            if (updatetower.levelOfTower < 2)
            {
                float realCurrentCooldown = currentBaseCooldown * GameManager.globalAttackSpeedMultiplier;
                float baseNextCooldown = currentBaseCooldown - upgradeCooldownStep;
                baseNextCooldown = Mathf.Max(baseNextCooldown, 0.1f);
                float realNextCooldown = baseNextCooldown * GameManager.globalAttackSpeedMultiplier;
                realNextCooldown = Mathf.Max(realNextCooldown, 0.1f);

                text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] -> <color=#2ECC71>[" + realNextCooldown.ToString("F2") + "s] </color>";
            }
            else
            {
                float realCurrentCooldown = currentBaseCooldown * GameManager.globalAttackSpeedMultiplier;
                text.text = "Recarga: [" + realCurrentCooldown.ToString("F2") + "s] (MÁXIMO)";
            }
        }
        else
        {
            if (updatetower.levelOfTower < 2)
            {
                float realCurrentScan = fireCooldown * GameManager.globalAttackSpeedMultiplier;
                float baseNextScan = fireCooldown - upgradeCooldownStep;
                baseNextScan = Mathf.Max(baseNextScan, 0.1f);
                float realNextScan = baseNextScan * GameManager.globalAttackSpeedMultiplier;
                realNextScan = Mathf.Max(realNextScan, 0.1f);

                text.text = "Escaneo: [" + realCurrentScan.ToString("F2") + "s] -> <color=#2ECC71>[" + realNextScan.ToString("F2") + "s] </color>";
            }
            else
            {
                float realCurrentScan = fireCooldown * GameManager.globalAttackSpeedMultiplier;
                text.text = "Escaneo: [" + realCurrentScan.ToString("F2") + "s] (MÁXIMO)";
            }
        }
    }

    public void RemoveAllBuffs()
    {
        foreach (Tower tower in buffedTowers)
        {
            if (tower != null)
            {
                tower.currentDamage -= currentIncreaseDamage;
            }
        }
        buffedTowers.Clear();
    }

    public float GetRealDamage(float baseDamage)
    {
        return baseDamage * GameManager.globalDamageTakenMultiplier;
    }

    public void DestroyByEnemy(GameObject destroyEffectPrefab, float destroyDelay)
    {
        if (!isBuilt) return;
        destroyedByEnemy = true;
        isBuilt = false;
        GameManager.countTower -= 1;

        if (config is SupportTowerData)
        {
            RemoveAllBuffs();
        }

        if (towerActiveInMenu == this)
        {
            setGameObjectUpDeleStatus(false);
        }

        if (destroyEffectPrefab != null)
        {
            GameObject fx = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, destroyDelay);
        }

        Destroy(gameObject, destroyDelay);
    }
}