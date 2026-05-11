using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la aparición de cartas de mejora, pausa el juego y aplica los beneficios.
/// Para añadir nuevas mejoras: añade un valor al enum UpgradeType, registra los datos
/// en InitializeUpgrades() e implementa la lógica en ApplyUpgrade().
/// </summary>
public class CardManager : MonoBehaviour
{
    [Header("UI de Cartas")]
    [Tooltip("El panel oscuro que agrupa las 3 cartas y tapa la pantalla.")]
    public GameObject cardsPanel;
    [Tooltip("Los 3 botones que representan las cartas.")]
    public Button[] cardButtons;
    [Tooltip("Textos para el título de las 3 cartas.")]
    public TextMeshProUGUI[] cardTitles;
    [Tooltip("Textos para la descripción de las 3 cartas.")]
    public TextMeshProUGUI[] cardDescriptions;

    [Header("Referencias Externas")]
    public castleScript castle;
    public GameManager gameManager;

    // ── Tipos de mejora disponibles ──────────────────────────────────────────

    public enum UpgradeType
    {
        HealCastle,
        DamageUp,
        AttackSpeedUp,
        RadiusUp,
        FireBurn       // ← nueva mejora de fuego
    }

    // ── Datos internos de carta ──────────────────────────────────────────────

    private struct CardData
    {
        public UpgradeType type;
        public string title;
        public string description;
    }

    private List<CardData> _availableUpgrades;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (cardsPanel != null) cardsPanel.SetActive(false);
        InitializeUpgrades();
    }

    // ── Mazo de cartas ───────────────────────────────────────────────────────

    /// <summary>
    /// Define el catálogo completo de cartas posibles.
    /// </summary>
    private void InitializeUpgrades()
    {
        _availableUpgrades = new List<CardData>
        {
            new CardData
            {
                type        = UpgradeType.HealCastle,
                title       = "Reparación",
                description = "Restaura 25 PV al castillo."
            },
            new CardData
            {
                type        = UpgradeType.DamageUp,
                title       = "Fuerza Bruta",
                description = "Los enemigos reciben 20% MÁS de daño."
            },
            new CardData
            {
                type        = UpgradeType.AttackSpeedUp,
                title       = "Recarga Ligera",
                description = "Las torres disparan un 15% más RÁPIDO."
            },
            new CardData
            {
                type        = UpgradeType.RadiusUp,
                title       = "Vista de Águila",
                description = "Aumenta un 20% el RADIO de visión de las torres."
            },
            new CardData
            {
                type        = UpgradeType.FireBurn,
                title       = "Brasas Eternas",
                description = "Tus proyectiles tienen un 30% de probabilidad de incendiar al enemigo, aplicando daño de fuego a lo largo del tiempo."
            }
        };
    }

    // ── Mostrar cartas ───────────────────────────────────────────────────────

    public void ShowCards()
    {
        cardsPanel.SetActive(true);

        List<CardData> pool         = new List<CardData>(_availableUpgrades);
        List<CardData> chosenCards  = new List<CardData>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            chosenCards.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            cardTitles[i].text       = chosenCards[i].title;
            cardDescriptions[i].text = chosenCards[i].description;

            cardButtons[i].onClick.RemoveAllListeners();
            UpgradeType typeToApply = chosenCards[i].type;
            cardButtons[i].onClick.AddListener(() => ApplyUpgrade(typeToApply));
        }
    }

    // ── Aplicar mejora ───────────────────────────────────────────────────────

    /// <summary>
    /// Aplica la mejora seleccionada, oculta el panel y reanuda el juego.
    /// </summary>
    private void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.HealCastle:
                castle.life += 25;
                break;

            case UpgradeType.DamageUp:
                GameManager.globalDamageTakenMultiplier += 0.2f;
                break;

            case UpgradeType.AttackSpeedUp:
                GameManager.globalAttackSpeedMultiplier *= 0.85f;
                GameManager.globalAttackSpeedMultiplier  =
                    Mathf.Max(GameManager.globalAttackSpeedMultiplier, 0.5f);
                break;

            case UpgradeType.RadiusUp:
                GameManager.globalRadiusMultiplier += 0.2f;
                break;

            case UpgradeType.FireBurn:
                ApplyFireBurnUpgrade();
                break;
        }

        GameManager.currentState = GameState.Playing;
        cardsPanel.SetActive(false);
    }

    /// <summary>
    /// Activa (o incrementa) la probabilidad global de quemadura en proyectiles.
    /// Cada vez que el jugador elige esta carta, la probabilidad sube un 15%
    /// hasta un máximo del 80%.
    /// </summary>
    private void ApplyFireBurnUpgrade()
    {
        const float incrementPerCard = 0.15f;
        const float maxProbability   = 0.80f;

        GameManager.globalBurnProbability =
            Mathf.Min(GameManager.globalBurnProbability + incrementPerCard, maxProbability);
    }
}
