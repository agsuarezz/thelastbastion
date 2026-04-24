using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Gestiona la aparición de cartas de mejora, pausa el juego y aplica los beneficios
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
    public castleScript castle; // Para poder curarlo
    public GameManager gameManager;

    // Lista de tipos de mejoras posibles
    public enum UpgradeType { HealCastle, DamageUp, AttackSpeedUp, RadiusUp }

    // Estructura para definir que hace cada carta
    private struct CardData
    {
        public UpgradeType type;
        public string title;
        public string description;
    }

    private List<CardData> availableUpgrades;

    private void Start()
    {
        if (cardsPanel != null) cardsPanel.SetActive(false);
        InitializeUpgrades();
    }

    /// <summary>
    /// Define el mazo de cartas posibles
    /// </summary>
    private void InitializeUpgrades()
    {
        availableUpgrades = new List<CardData>
        {
            new CardData { type = UpgradeType.HealCastle, title = "Reparación", description = "Restaura 25 PV al castillo." },
            new CardData { type = UpgradeType.DamageUp, title = "Fuerza Bruta", description = "Los enemigos reciben 20% MÁS de daño." },
            new CardData { type = UpgradeType.AttackSpeedUp, title = "Recarga Ligera", description = "Las torres disparan un 15% más RÁPIDO." },
            new CardData { type = UpgradeType.RadiusUp, title = "Vista de Águila", description = "Aumenta un 20% el RADIO de visión de las torres." }
        };
    }

    public void ShowCards()
    {
        cardsPanel.SetActive(true);
        Time.timeScale = 0f;

        // Seleccionar 3 cartas aleatorias sin que se repitan
        List<CardData> pool = new List<CardData>(availableUpgrades);
        List<CardData> chosenCards = new List<CardData>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            chosenCards.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex); // Se quita del pool para que no se repita
        }

        for (int i = 0; i < 3; i++)
        {
            cardTitles[i].text = chosenCards[i].title;
            cardDescriptions[i].text = chosenCards[i].description;

            cardButtons[i].onClick.RemoveAllListeners();
            UpgradeType typeToApply = chosenCards[i].type;
            cardButtons[i].onClick.AddListener(() => ApplyUpgrade(typeToApply));
        }
        
    }

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
                break;
            case UpgradeType.RadiusUp:
                GameManager.globalRadiusMultiplier += 0.2f;
                break;
        }

        GameManager.currentState = GameState.Playing;
        cardsPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}