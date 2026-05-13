using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour
{
    [Header("Mis Datos")]
    public SkillData myData; // Arrastras el ScriptableObject aquí en el Inspector

    [Header("Mi UI")]
    public Image iconImage;
    public TextMeshProUGUI costText;
    public Button buyButton;
    public LineRenderer lineRenderer;

    [Header("Ajustes Visuales del Árbol")]
    private Color colorLocked = Color.red;
    private Color colorUnlocked = new Color(0.4f, 0.8f, 0.2f, 1f);
    void Start()
    {
        // Al arrancar, el botón se disfraza con los datos del ScriptableObject
        iconImage.sprite = myData.icon;
        costText.text = myData.baseCost.ToString() + " XP";
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnClick);
        }
    }

    public void OnClick()
    {
        // Le pasamos nuestro ScriptableObject al Manager para que él haga los cálculos
        FindObjectOfType<SkillTreeManager>().TryBuySkill(myData);
    }

    /// <summary>
    /// El Manager llama a esto para decirle al botón cómo tiene que verse.
    /// </summary>
    public void RefreshVisuals(bool isUnlocked, int currentLevel, int currentCost)
    {
        if (!isUnlocked)
        {
            // ESTADO 1: NO SE PUEDE COMPRAR
            if (buyButton != null) buyButton.interactable = false; // No se puede clickear
            if (costText != null) costText.text = "";
            // Oscurecemos el icono
            iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            // Ponemos la línea "bloqueada"
            if (lineRenderer != null)
            {
                lineRenderer.startColor = colorLocked;
                lineRenderer.endColor = colorLocked;
            }
        }
        else if(currentLevel >= myData.maxNBuy)
        {
            // ESTADO 3: COMPRADO AL MÁXIMO
            if (buyButton != null) buyButton.interactable = true;
            if (costText != null) costText.text = "MÁXIMO";
            iconImage.color = Color.white;

            if (lineRenderer != null)
            {
                lineRenderer.startColor = colorUnlocked;
                lineRenderer.endColor = colorUnlocked;
            }
        }
        else
        {
            // ESTADO 2: NO LO TENEMOS, PERO PODEMOS COMPRARLO
            if (buyButton != null) buyButton.interactable = true; // Se puede clickear
            if (costText != null) costText.text = myData.baseCost.ToString() + " xp";
            // Icono visible pero indicando que faltaa dinero
            iconImage.color = Color.white;
            // Ponemos la línea "desbloqueada"
            if (lineRenderer != null)
            {
                lineRenderer.startColor = colorUnlocked;
                lineRenderer.endColor = colorUnlocked;
            }
        }
    }

}
