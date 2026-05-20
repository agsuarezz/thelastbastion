using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI del Árbol")]
    public TextMeshProUGUI totalXpText; // Texto donde dice "XP: 84"

    public AudioClip soundFailBuy;
    public AudioClip soundBuy;
    public AudioSource audioSource;

    // Una lista con todos los botones de tu escena para poder avisarles de que se actualicen
    public Transform nodes;
    private List<SkillNode> allNodesInTree = new List<SkillNode>();

    private MetaSaveData currentMeta; // Los datos del jugador cargados en memoria

    public TextMeshProUGUI messageErrorText;

    private void Start()
    {
        SaveSystem.DeleteMeta();
        // 1. Al abrir la escena del árbol, cargamos la partida
        currentMeta = SaveSystem.LoadMeta();
        currentMeta.totalExperience += 100;

        // 2. Obtenemos todos los nodos
        if (nodes != null)
        {
            allNodesInTree.AddRange(nodes.GetComponentsInChildren<SkillNode>());
        }

        // 3. Iniciamos todos los nodos en el guardado si no lo están
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

        // 4. Actualizamos todo visualmente
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
            StartCoroutine(messageError("Límite alcanzado. Si mejoramos esto un nivel más, el motor de Unity explota y te borra el Windows."));
            audioSource.PlayOneShot(soundFailBuy);
            return;
        }

        int cost = skillToBuy.baseCost + (currentLevel * skillToBuy.costMultiplier);
        if (currentMeta.totalExperience < cost)
        {
            StartCoroutine(messageError("Sin experiencia no hay mejoras. Si las quieres gratis, haber comprado la edición Deluxe."));
            audioSource.PlayOneShot(soundFailBuy);
            return;
        }

        // --- ¡COMPRA ACEPTADA! ---

        // Descontamos la XP
        currentMeta.totalExperience -= cost;

        // Desbloqueamos la torre correspondiente en nuestro MetaSaveData
        UpLevel(skillToBuy.skillID);

        // Se aplica el efecto
        ApplyUpgrade(skillToBuy);
        audioSource.PlayOneShot(soundBuy);

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
            // Pillamos el nivel actual de este nodo para pasárselo a la UI
            int currentLevel = GetSkillLevel(node.myData.skillID);

            // Calculamos el coste actual para mostrarlo bien en el texto
            int currentCost = node.myData.baseCost + (currentLevel * node.myData.costMultiplier);
            if (node.myData.prerequisite != null)
            {
                SkillProgress nodePrerequisite = GetNode(node.myData.prerequisite.skillID);
                bool isUnlocked = (nodePrerequisite != null) && nodePrerequisite.buyed;
                node.RefreshVisuals(isUnlocked, currentLevel, currentCost);
            }
            else
            {
                node.RefreshVisuals(true, currentLevel, currentCost);
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
        int index = node.typeUpgrade - 1;
        string towerName = "Torre " + node.typeTower.ToString();

        // Comprobamos que el diccionario tiene la torre y que el índice es seguro (0, 1 o 2)
        if (currentMeta.upgradesTree.ContainsKey(towerName) && index >= 0 && index < currentMeta.upgradesTree[towerName].Count)
        {
            currentMeta.upgradesTree[towerName][index] += node.benefitPerLevel;
        }
        else
        {
            Debug.LogError($"[Árbol] Configuración inválida en la carta {node.skillID}. Índice {index} fuera de rango.");
        }
    }
    public void startGame()
    {
        SceneManager.LoadScene("Main");
    }
    /// <summary>
    /// Muestra un mensaje de error en pantalla durante 2 segundos y luego lo borra.
    /// </summary>
    public IEnumerator messageError(string text)
    {
        messageErrorText.text = text;
        messageErrorText.color = Color.red;
        yield return new WaitForSeconds(2f);
        messageErrorText.text = "";
    }

}
