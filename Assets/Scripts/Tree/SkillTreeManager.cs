using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI del Árbol")]
    public TextMeshProUGUI totalXpText; // Texto donde dice "XP: 84"

    // Una lista con todos los botones de tu escena para poder avisarles de que se actualicen
    public List<SkillNode> allNodesInTree;

    private MetaSaveData currentMeta; // Los datos del jugador cargados en memoria

    private void Start()
    {
        // 1. Al abrir la escena del árbol, cargamos la partida
        currentMeta = SaveSystem.LoadMeta();

        // 2. Actualizamos todo visualmente
        UpdateUI();
    }


    /// <summary>
    /// Método que llama un SkillNode cuando el jugador hace click en él.
    /// </summary>
    public void TryBuySkill(SkillData skillToBuy)
    {

        // 1. ¿Tenemos el requisito previo comprado? (Si es que tiene)
        if (skillToBuy.prerequisite != null && !skillToBuy.prerequisite.buyed)
        {
            Debug.LogWarning("Te falta comprar la mejora anterior primero.");
            return;
        }

        // 2. ¿Tenemos suficiente pasta (XP)?
        if (currentMeta.totalExperience < skillToBuy.cost)
        {
            Debug.LogWarning("No tienes suficiente XP. ¡A farmear!");
            // Aquí podrías reproducir el 'GameManager.soundError'
            return;
        }

        // 3.. ¿Ya está comprado?
        if (skillToBuy.buyed)
        {
            Debug.LogWarning("Eso ya lo compraste, tontín.");
            return;
        }

        // --- ¡COMPRA ACEPTADA! ---

        // Descontamos la XP
        currentMeta.totalExperience -= skillToBuy.cost;

        // Desbloqueamos la torre correspondiente en nuestro MetaSaveData
        skillToBuy.buyed = true;

        // Guardamos físicamente en el archivo JSON
        SaveSystem.SaveMeta(currentMeta);

        // Reproducimos sonido de compra
        GameManager.sound(GameManager.soundPay);

        // Actualizamos todos los botones y textos
        UpdateUI();
    }

    /// <summary>
    /// Actualiza el texto de XP y manda a todos los botones a repintarse.
    /// </summary>
    private void UpdateUI()
    {
        totalXpText.text = "XP Disponible: " + currentMeta.totalExperience;

        // Recorremos todos los botones y les decimos: "Oye, revisa si estás bloqueado o comprado"
        foreach (SkillNode node in allNodesInTree)
        {
            if (node.myData.prerequisite) node.RefreshVisuals(node.myData.prerequisite.buyed);
            else node.RefreshVisuals(true);
        }
    }
}
