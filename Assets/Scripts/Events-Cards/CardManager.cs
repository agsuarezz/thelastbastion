using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la aparición de cartas de mejora, pausa el juego y aplica los beneficios.
/// Las mejoras de torre se aplican a TODAS las torres de un tipo específico (presentes y futuras).
/// </summary>
public class CardManager : MonoBehaviour
{
    [Header("UI de Cartas")]
    public GameObject cardsPanel;
    public Button[] cardButtons;
    public TextMeshProUGUI[] cardTitles;
    public TextMeshProUGUI[] cardDescriptions;

    [Header("Referencias Externas")]
    public castleScript castle;
    public GameManager gameManager;

    [Header("Catálogo de Torres")]
    [Tooltip("Arrastra aquí TODOS los ScriptableObjects de tus torres (TowerData) para que puedan salir en las cartas.")]
    public List<TowerData> allTowerTypes;

    // ── Tipos de mejora disponibles ──────────────────────────────────────────

    public enum UpgradeType
    {
        HealCastle,
        DamageUp,
        AttackSpeedUp,
        RadiusUp,
        FireBurn,
        SlowEnemies,
        PoisonStrike,
        Greed,
        ChainLightning
    }

    private struct CardData
    {
        public UpgradeType type;
        public string title;
        public string description;
    }

    private List<CardData> _availableUpgrades;

    // ── Diccionarios de Mejoras Globales por Tipo de Torre ───────────────────
    
    public static Dictionary<string, float> towerDamageMultipliers = new Dictionary<string, float>();
    public static Dictionary<string, float> towerSpeedMultipliers  = new Dictionary<string, float>();
    public static Dictionary<string, float> towerRadiusMultipliers = new Dictionary<string, float>();
    public static Dictionary<string, float> towerBurnBonus         = new Dictionary<string, float>();
    public static Dictionary<string, float> towerPoisonBonus       = new Dictionary<string, float>();
    public static Dictionary<string, float> towerChainBonus        = new Dictionary<string, float>();

    /// <summary>
    /// Asegura que el tipo de torre tenga sus contadores inicializados a los valores base.
    /// </summary>
    public static void InitializeTowerDict(string name)
    {
        if (!towerDamageMultipliers.ContainsKey(name))
        {
            towerDamageMultipliers[name] = 1f;
            towerSpeedMultipliers[name]  = 1f;
            towerRadiusMultipliers[name] = 1f;
            towerBurnBonus[name]         = 0f;
            towerPoisonBonus[name]       = 0f;
            towerChainBonus[name]        = 0f;
        }
    }

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (cardsPanel != null) cardsPanel.SetActive(false);
        InitializeUpgrades();

        // IMPORTANTE: Limpiamos los diccionarios por si se reinicia el nivel
        towerDamageMultipliers.Clear();
        towerSpeedMultipliers.Clear();
        towerRadiusMultipliers.Clear();
        towerBurnBonus.Clear();
        towerPoisonBonus.Clear();
        towerChainBonus.Clear();
    }

    private void InitializeUpgrades()
    {
        _availableUpgrades = new List<CardData>
        {
            new CardData { type = UpgradeType.HealCastle, title = "Reparación", description = "Restaura 25 PV al castillo." },
            new CardData { type = UpgradeType.DamageUp, title = "Fuerza Bruta", description = "Una torre aleatoria inflige un 20% MÁS de daño." },
            new CardData { type = UpgradeType.AttackSpeedUp, title = "Recarga Ligera", description = "Una torre aleatoria dispara un 15% más RÁPIDO." },
            new CardData { type = UpgradeType.RadiusUp, title = "Vista de Águila", description = "Una torre aleatoria aumenta su RADIO de visión un 20%." },
            new CardData { type = UpgradeType.FireBurn, title = "Brasas Eternas", description = "Una torre aleatoria tiene un 15% más de probabilidad de INCENDIAR al enemigo." },
            new CardData { type = UpgradeType.SlowEnemies, title = "Pantano Espeso", description = "Todos los enemigos se mueven un 20% más LENTO de forma permanente." },
            new CardData { type = UpgradeType.PoisonStrike, title = "Flechas Envenenadas", description = "Una torre aleatoria tiene un 25% más de probabilidad de ENVENENAR al enemigo." },
            new CardData { type = UpgradeType.Greed, title = "Codicia", description = "Ganas un 15% MÁS de oro por cada enemigo eliminado." },
            new CardData { type = UpgradeType.ChainLightning, title = "Tormenta Eléctrica", description = "Una torre aleatoria tiene un 30% más de probabilidad de generar una CADENA ELÉCTRICA." }
        };
    }

    // ── Mostrar cartas ───────────────────────────────────────────────────────

    public void ShowCards()
    {
        cardsPanel.SetActive(true);

        List<CardData> pool = new List<CardData>(_availableUpgrades);
        List<CardData> chosenCards = new List<CardData>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            chosenCards.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            UpgradeType typeToApply = chosenCards[i].type;
            string finalDescription = chosenCards[i].description;
            TowerData preSelectedTowerData = null;

            if (typeToApply == UpgradeType.DamageUp || typeToApply == UpgradeType.AttackSpeedUp ||
                typeToApply == UpgradeType.RadiusUp || typeToApply == UpgradeType.FireBurn ||
                typeToApply == UpgradeType.PoisonStrike || typeToApply == UpgradeType.ChainLightning)
            {
                preSelectedTowerData = GetRandomEligibleTowerData();

                if (preSelectedTowerData != null)
                {
                    // Formateamos gramaticalmente para que encaje como tipo de torre
                    string towerName = "El tipo de torre <b><color=#F39C12>" + preSelectedTowerData.nameOfTower + "</color></b>";
                    finalDescription = finalDescription.Replace("Una torre aleatoria", towerName);
                }
                else
                {
                    finalDescription = finalDescription.Replace("Una torre aleatoria", "<b>Tu defensa global</b>");
                }
            }

            cardTitles[i].text = chosenCards[i].title;
            cardDescriptions[i].text = finalDescription;

            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => ApplyUpgrade(typeToApply, preSelectedTowerData));
        }
    }

    // ── Selección de TIPO de torre aleatoria ─────────────────────────────────

    /// <summary>
    /// Devuelve un TowerData aleatorio de entre los que el jugador haya desbloqueado,
    /// buscando en la lista pública allTowerTypes.
    /// </summary>
    private TowerData GetRandomEligibleTowerData()
    {
        if (allTowerTypes == null || allTowerTypes.Count == 0) return null;

        List<TowerData> eligible = new List<TowerData>();

        foreach (TowerData td in allTowerTypes)
        {
            // Solo incluimos tipos de torre que el jugador haya desbloqueado
            if (TowerLockUI.IsTowerUnlocked(td))
            {
                eligible.Add(td);
            }
        }

        if (eligible.Count == 0) return null;

        return eligible[Random.Range(0, eligible.Count)];
    }

    // ── Aplicar mejora ───────────────────────────────────────────────────────

    private void ApplyUpgrade(UpgradeType type, TowerData targetTowerData = null)
    {
        switch (type)
        {
            case UpgradeType.HealCastle:
                castle.life += 25;
                break;
            case UpgradeType.SlowEnemies:
                ApplySlowEnemiesUpgrade();
                break;
            case UpgradeType.Greed:
                ApplyGreedUpgrade();
                break;
            case UpgradeType.DamageUp:
                ApplyTowerDamageUp(targetTowerData);
                break;
            case UpgradeType.AttackSpeedUp:
                ApplyTowerAttackSpeedUp(targetTowerData);
                break;
            case UpgradeType.RadiusUp:
                ApplyTowerRadiusUp(targetTowerData);
                break;
            case UpgradeType.FireBurn:
                ApplyTowerFireBurn(targetTowerData);
                break;
            case UpgradeType.PoisonStrike:
                ApplyTowerPoisonStrike(targetTowerData);
                break;
            case UpgradeType.ChainLightning:
                ApplyTowerChainLightning(targetTowerData);
                break;
        }

        GameManager.currentState = GameState.Playing;
        cardsPanel.SetActive(false);
    }

    // ── Mejoras de tipo de torre ─────────────────────────────────────────────

    private void ApplyTowerDamageUp(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerDamageMultipliers[name] *= 1.20f; // Guardamos para el futuro

            // Aplicamos retroactivamente a las que YA están en el tablero
            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    if (t.config is SupportTowerData) t.currentIncreaseDamage *= 1.20f;
                    else t.currentDamage *= 1.20f;
                    
                    t.upgradeDamageStep *= 1.20f; // Mejoramos el paso para futuras evoluciones
                    NotifyTowerUpgrade(t, "¡Daño +20%!");
                }
            }
        }
        else
        {
            GameManager.globalDamageTakenMultiplier += 0.2f;
        }
    }

    private void ApplyTowerAttackSpeedUp(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerSpeedMultipliers[name] *= 0.85f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.ApplyLocalSpeedBonus(0.85f);
                    t.upgradeCooldownStep *= 0.85f;
                    NotifyTowerUpgrade(t, "¡Velocidad +15%!");
                }
            }
        }
        else
        {
            GameManager.globalAttackSpeedMultiplier *= 0.85f;
            GameManager.globalAttackSpeedMultiplier = Mathf.Max(GameManager.globalAttackSpeedMultiplier, 0.5f);
        }
    }

    private void ApplyTowerRadiusUp(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerRadiusMultipliers[name] *= 1.20f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.attackRadius *= 1.20f;
                    NotifyTowerUpgrade(t, "¡Radio +20%!");
                }
            }
        }
        else
        {
            GameManager.globalRadiusMultiplier += 0.2f;
        }
    }

    private void ApplyTowerFireBurn(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerBurnBonus[name] += 0.15f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localBurnBonus = Mathf.Min(t.localBurnBonus + 0.15f, 0.80f);
                    NotifyTowerUpgrade(t, "¡Fuego +15%!");
                }
            }
        }
        else
        {
            GameManager.globalBurnProbability = Mathf.Min(GameManager.globalBurnProbability + 0.15f, 0.80f);
        }
    }

    private void ApplyTowerPoisonStrike(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerPoisonBonus[name] += 0.25f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localPoisonBonus = Mathf.Min(t.localPoisonBonus + 0.25f, 0.80f);
                    NotifyTowerUpgrade(t, "¡Veneno +25%!");
                }
            }
        }
        else
        {
            GameManager.globalPoisonProbability = Mathf.Min(GameManager.globalPoisonProbability + 0.25f, 0.80f);
        }
    }

    private void ApplyTowerChainLightning(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerChainBonus[name] += 0.30f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localChainBonus = Mathf.Min(t.localChainBonus + 0.30f, 0.80f);
                    NotifyTowerUpgrade(t, "¡Rayo +30%!");
                }
            }
        }
        else
        {
            GameManager.globalChainLightningChance = Mathf.Min(GameManager.globalChainLightningChance + 0.80f, 0.80f);
        }
    }

    // ── Mejoras globales (sin cambios) ───────────────────────────────────────

    private void ApplySlowEnemiesUpgrade()
    {
        const float slowPerCard = 0.20f;
        const float minSpeedMult = 0.30f;
        GameManager.globalSpeedMultiplier = Mathf.Max(GameManager.globalSpeedMultiplier - slowPerCard, minSpeedMult);
    }

    private void ApplyGreedUpgrade()
    {
        const float bonusPerCard = 0.15f;
        GameManager.globalMoneyBonusMultiplier += bonusPerCard;
    }

    private void NotifyTowerUpgrade(Tower tower, string message)
    {
        Debug.Log($"[CardManager] Mejora aplicada a '{tower.config.nameOfTower}' en {tower.transform.position}: {message}");
    }
}