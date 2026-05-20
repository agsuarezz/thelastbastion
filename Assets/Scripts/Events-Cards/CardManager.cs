using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la aparición de cartas de mejora, pausa el juego y aplica los beneficios.
/// Las mejoras de torre se aplican a TODAS las torres de un tipo específico (presentes y futuras).
/// Ahora incluye filtros automáticos para que no aparezcan mejoras que ya han alcanzado su límite máximo.
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

    // ── Filtros Lógicos para evitar Cartas Inútiles ──────────────────────────

    /// <summary>
    /// Comprueba si esta mejora ya ha llegado a su tope en TODOS los frentes (global y todas las torres posibles).
    /// </summary>
    private bool IsUpgradeAvailable(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.SlowEnemies:
                // 0.3f es la velocidad mínima, si ya está ahí o menos, la descartamos.
                return GameManager.globalSpeedMultiplier > 0.30f;
                
            case UpgradeType.FireBurn:
                if (GameManager.globalBurnProbability < 0.80f) return true;
                return HasAnyTowerNotMaxed(towerBurnBonus, 0.80f);

            case UpgradeType.PoisonStrike:
                if (GameManager.globalPoisonProbability < 0.80f) return true;
                return HasAnyTowerNotMaxed(towerPoisonBonus, 0.80f);

            case UpgradeType.ChainLightning:
                if (GameManager.globalChainLightningChance < 0.80f) return true;
                return HasAnyTowerNotMaxed(towerChainBonus, 0.80f);

            default:
                // Vida, oro, daño, velocidad de ataque y radio no tienen límite programado
                return true;
        }
    }

    /// <summary>
    /// Revisa si hay AL MENOS UNA torre desbloqueada que aún no haya llegado al máximo en una estadística concreta.
    /// </summary>
    private bool HasAnyTowerNotMaxed(Dictionary<string, float> dict, float maxVal)
    {
        if (allTowerTypes == null) return false;
        foreach (var td in allTowerTypes)
        {
            if (TowerLockUI.IsTowerUnlocked(td))
            {
                string name = td.nameOfTower;
                InitializeTowerDict(name);
                // Usamos un pequeño margen (0.01f) para evitar errores de coma flotante
                if (dict[name] < maxVal - 0.01f) return true; 
            }
        }
        return false;
    }

    // ── Mostrar cartas ───────────────────────────────────────────────────────

    public void ShowCards()
    {
        cardsPanel.SetActive(true);

        List<CardData> pool = new List<CardData>();
        
        // 1. Filtramos el pool para meter SOLAMENTE las cartas que aún no estén al máximo
        foreach (var card in _availableUpgrades)
        {
            if (IsUpgradeAvailable(card.type))
            {
                pool.Add(card);
            }
        }

        List<CardData> chosenCards = new List<CardData>();
        
        // En caso extremadamente raro de que queden menos de 3 opciones útiles en todo el juego, ajustamos
        int cardsToPick = Mathf.Min(3, pool.Count);

        for (int i = 0; i < cardsToPick; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            chosenCards.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            if (i < chosenCards.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                UpgradeType typeToApply = chosenCards[i].type;
                string finalDescription = chosenCards[i].description;
                TowerData preSelectedTowerData = null;

                if (typeToApply == UpgradeType.DamageUp || typeToApply == UpgradeType.AttackSpeedUp ||
                    typeToApply == UpgradeType.RadiusUp || typeToApply == UpgradeType.FireBurn ||
                    typeToApply == UpgradeType.PoisonStrike || typeToApply == UpgradeType.ChainLightning)
                {
                    // Pasamos el tipo de mejora para que NO elija una torre que ya lo tenga al máximo
                    preSelectedTowerData = GetRandomEligibleTowerData(typeToApply);

                    if (preSelectedTowerData != null)
                    {
                        string towerName = "El tipo de torre <b><color=#F39C12>" + preSelectedTowerData.nameOfTower + "</color></b>";
                        finalDescription = finalDescription.Replace("Una torre aleatoria", towerName);
                    }
                    else
                    {
                        finalDescription = finalDescription.Replace("Una torre aleatoria", "<b>Tu defensa global</b>");
                    }
                }

                // Añadimos las estadísticas de progreso
                finalDescription += GetUpgradeStatsText(typeToApply, preSelectedTowerData);

                cardTitles[i].text = chosenCards[i].title;
                cardDescriptions[i].text = finalDescription;

                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => ApplyUpgrade(typeToApply, preSelectedTowerData));
            }
            else
            {
                // Apagamos el botón si no quedan opciones (caso hiper extremo en late-game)
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ── Selección de TIPO de torre aleatoria ─────────────────────────────────

    /// <summary>
    /// Devuelve un TowerData aleatorio de entre los que el jugador haya desbloqueado,
    /// ASEGURÁNDOSE de que esa torre no tenga ya al máximo la estadística que se va a mejorar.
    /// </summary>
    private TowerData GetRandomEligibleTowerData(UpgradeType type)
    {
        if (allTowerTypes == null || allTowerTypes.Count == 0) return null;

        List<TowerData> eligible = new List<TowerData>();

        foreach (TowerData td in allTowerTypes)
        {
            if (TowerLockUI.IsTowerUnlocked(td))
            {
                string name = td.nameOfTower;
                InitializeTowerDict(name);
                
                bool canUpgrade = true;
                
                // Excluimos la torre si la carta elegida es de límite estricto y ya lo alcanzó localmente
                if (type == UpgradeType.FireBurn && towerBurnBonus[name] >= 0.79f) canUpgrade = false;
                if (type == UpgradeType.PoisonStrike && towerPoisonBonus[name] >= 0.79f) canUpgrade = false;
                if (type == UpgradeType.ChainLightning && towerChainBonus[name] >= 0.79f) canUpgrade = false;

                if (canUpgrade) eligible.Add(td);
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
            towerDamageMultipliers[name] *= 1.20f;

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    if (t.config is SupportTowerData) t.currentIncreaseDamage *= 1.20f;
                    else t.currentDamage *= 1.20f;
                    
                    t.upgradeDamageStep *= 1.20f;
                    NotifyTowerUpgrade(t, "¡Daño +20%!");
                }
            }
        }
        else GameManager.globalDamageTakenMultiplier += 0.2f;
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
        else GameManager.globalRadiusMultiplier += 0.2f;
    }

    private void ApplyTowerFireBurn(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            // Capamos el diccionario explícitamente para que el filtro lo detecte
            towerBurnBonus[name] = Mathf.Min(towerBurnBonus[name] + 0.15f, 0.80f);

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localBurnBonus = towerBurnBonus[name];
                    NotifyTowerUpgrade(t, "¡Fuego +15%!");
                }
            }
        }
        else GameManager.globalBurnProbability = Mathf.Min(GameManager.globalBurnProbability + 0.15f, 0.80f);
    }

    private void ApplyTowerPoisonStrike(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerPoisonBonus[name] = Mathf.Min(towerPoisonBonus[name] + 0.25f, 0.80f);

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localPoisonBonus = towerPoisonBonus[name];
                    NotifyTowerUpgrade(t, "¡Veneno +25%!");
                }
            }
        }
        else GameManager.globalPoisonProbability = Mathf.Min(GameManager.globalPoisonProbability + 0.25f, 0.80f);
    }

    private void ApplyTowerChainLightning(TowerData td)
    {
        if (td != null)
        {
            string name = td.nameOfTower;
            InitializeTowerDict(name);
            towerChainBonus[name] = Mathf.Min(towerChainBonus[name] + 0.30f, 0.80f);

            Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
            foreach (Tower t in allTowers)
            {
                if (t.isBuilt && t.config != null && t.config.nameOfTower == name)
                {
                    t.localChainBonus = towerChainBonus[name];
                    NotifyTowerUpgrade(t, "¡Rayo +30%!");
                }
            }
        }
        else GameManager.globalChainLightningChance = Mathf.Min(GameManager.globalChainLightningChance + 0.30f, 0.80f);
    }

    // ── Mejoras globales ───────────────────────────────────────

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

    // ── Textos de Estadísticas de Cartas ─────────────────────────────────────

    private string GetUpgradeStatsText(UpgradeType type, TowerData td)
    {
        string statText = "";
        string towerName = td != null ? td.nameOfTower : "";

        if (td != null) InitializeTowerDict(towerName);

        string colorHex = "#2ECC71";

        switch (type)
        {
            case UpgradeType.FireBurn:
                float currentFire = td != null ? towerBurnBonus[towerName] : GameManager.globalBurnProbability;
                statText = $"\n<size=80%><color={colorHex}>(Actual: {Mathf.RoundToInt(currentFire * 100)}% / Máx: 80%)</color></size>";
                break;

            case UpgradeType.PoisonStrike:
                float currentPoison = td != null ? towerPoisonBonus[towerName] : GameManager.globalPoisonProbability;
                statText = $"\n<size=80%><color={colorHex}>(Actual: {Mathf.RoundToInt(currentPoison * 100)}% / Máx: 80%)</color></size>";
                break;

            case UpgradeType.ChainLightning:
                float currentChain = td != null ? towerChainBonus[towerName] : GameManager.globalChainLightningChance;
                statText = $"\n<size=80%><color={colorHex}>(Actual: {Mathf.RoundToInt(currentChain * 100)}% / Máx: 80%)</color></size>";
                break;

            case UpgradeType.SlowEnemies:
                float currentSlow = (1f - GameManager.globalSpeedMultiplier) * 100f;
                if (currentSlow < 0) currentSlow = 0; 
                statText = $"\n<size=80%><color={colorHex}>(Actual: {Mathf.RoundToInt(currentSlow)}% / Máx: 70%)</color></size>";
                break;

            case UpgradeType.DamageUp:
            case UpgradeType.RadiusUp:
            case UpgradeType.AttackSpeedUp:
            case UpgradeType.Greed:
                statText = $"\n<size=80%><color={colorHex}>(Máx: Sin límite)</color></size>";
                break;
        }

        return statText;
    }
}