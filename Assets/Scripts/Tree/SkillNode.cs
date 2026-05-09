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
        }
        else
        {
            // ESTADO 2: NO LO TENEMOS, PERO PODEMOS COMPRARLO
            if (buyButton != null) buyButton.interactable = true; // Se puede clickear
            if (costText != null) costText.text = myData.cost.ToString() + " xp";
        }
    }

}
