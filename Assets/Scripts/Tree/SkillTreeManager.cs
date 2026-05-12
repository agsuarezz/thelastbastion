using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
        SaveSystem.DeleteMeta();
        // 1. Al abrir la escena del árbol, cargamos la partida
        currentMeta = SaveSystem.LoadMeta();
        currentMeta.totalExperience += 100;

        // 2. Iniciamos todos los nodos en el guardado si no lo están
        bool confirmed = false;
        foreach (SkillNode node in allNodesInTree)
        {
            if (GetNode(node.myData.skillID) == null)
            {
                InitNode(node);
                confirmed = true;
            }
        }
        if (confirmed) SaveSystem.SaveMeta(currentMeta);
        SaveSystem.DebugLogMetaSave();

        // 3. Actualizamos todo visualmente
        UpdateUI();
    }


    /// <summary>
    /// Método que llama un SkillNode cuando el jugador hace click en él.
    /// </summary>
    public void TryBuySkill(SkillData skillToBuy)
    {

        int currentLevel = GetSkillLevel(skillToBuy.skillID);
        if (currentLevel >= skillToBuy.maxNBuy)
        {
            Debug.LogWarning("Ya se ha comprado este nodo al máximo.");
            return;
        }

        int cost = skillToBuy.baseCost + (currentLevel * skillToBuy.costMultiplier);
        if (currentMeta.totalExperience < cost)
        {
            Debug.LogWarning("No hay pasta.");
            return;
        }

        // --- ¡COMPRA ACEPTADA! ---

        // Descontamos la XP
        currentMeta.totalExperience -= cost;

        // Desbloqueamos la torre correspondiente en nuestro MetaSaveData
        UpLevel(skillToBuy.skillID);

        // Se aplica el efecto
        ApplyUpgrade(skillToBuy);

        // Guardamos físicamente en el archivo JSON
        SaveSystem.SaveMeta(currentMeta);
        SaveSystem.DebugLogMetaSave();

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
            if (node.myData.prerequisite != null)
            {
                SkillProgress nodePrerequisite = GetNode(node.myData.prerequisite.skillID);
                node.RefreshVisuals(nodePrerequisite.buyed);
            }
            else
            {
                node.RefreshVisuals(true);
                Debug.Log(node.myData.skillID);
            }
        }
    }

    private void InitNode(SkillNode node)
    {
        currentMeta.skillList.Add(new SkillProgress { id = node.myData.skillID, level = 0, buyed = node.myData.maxNBuy == 0 });
    }

    public int GetSkillLevel(string id)
    {
        // Buscamos en la lista si existe una entrada con ese ID
        SkillProgress progress = GetNode(id);

        if (progress != null) return progress.level;
        return 0;
    }

    public void UpLevel(string id)
    {
        SkillProgress progress = GetNode(id);
        int maxNBuy = allNodesInTree.Find(s => s.myData.skillID == id).myData.maxNBuy;

        if (progress != null)
        {
            progress.level += 1; // Si existe, actualizamos
            progress.buyed = progress.level == maxNBuy;
        }
        else
        {
            // Si no existe, creamos la entrada nueva en la lista
            currentMeta.skillList.Add(new SkillProgress { id = id, level = 1, buyed = (1 == maxNBuy) });
        }
    }
    public SkillProgress GetNode(string id)
    {
        return currentMeta.skillList.Find(s => s.id == id);
    }

    public void ApplyUpgrade(SkillData node)
    {
        if(node.typeUpgrade == 0)
        {
            if (node.typeTower == tower.Infernal)
            {
                currentMeta.isInfernalTowerUnlocked = true;
            }
            else
            {
                currentMeta.isSupportTowerUnlocked = true;
            }
            return;
        }
        currentMeta.upgradesTree["Torre " + node.typeTower.ToString()][node.typeUpgrade - 1] += node.benefitPerLevel;
    }
}
