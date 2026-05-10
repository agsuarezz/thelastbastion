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
    private Color colorUnlocked = Color.red;
    private Color colorLocked = new Color(0.4f, 0.8f, 0.2f, 1f);
    void Start()
    {
        // Al arrancar, el botón se disfraza con los datos del ScriptableObject
        iconImage.sprite = myData.icon;
        costText.text = myData.cost.ToString() + " XP";
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
    public void RefreshVisuals(bool isUnlocked)
    {
        if (!isUnlocked)
        {
            // ESTADO 1: YA LO HEMOS COMPRADO
            if (buyButton != null) buyButton.interactable = false; // Ya no se puede clickear
            if (costText != null) costText.text = "";
            if (myData.prerequisite != null && !myData.prerequisite.buyed) iconImage.color = new Color(255f, 255f, 255f, 255f);
            // Ponemos la línea "encendida"
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
            if (costText != null) costText.text = myData.cost.ToString() + " xp";
            if (myData.prerequisite != null && !myData.prerequisite.buyed) iconImage.color = new Color(255f, 255f, 255f, 100f);
            // Ponemos la línea "apagada"
            if (lineRenderer != null)
            {
                lineRenderer.startColor = colorLocked;
                lineRenderer.endColor = colorLocked;
            }
        }
    }

}
