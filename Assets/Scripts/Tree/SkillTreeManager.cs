using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI del �rbol")]
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
        // 1. Al abrir la escena del �rbol, cargamos la partida
        currentMeta = SaveSystem.LoadMeta();

        // 2. Obtenemos todos los nodos
        if (nodes != null)
        {
            allNodesInTree.AddRange(nodes.GetComponentsInChildren<SkillNode>());
        }

        // 3. Iniciamos todos los nodos en el guardado si no lo est�n
        InitTree();
        SaveSystem.DebugLogMetaSave();

        // 4. Actualizamos todo visualmente
        UpdateUI();
    }

    /// <summary>
    /// M�todo que llama un SkillNode cuando el jugador hace click en �l.
    /// </summary>
    public void TryBuySkill(SkillData skillToBuy)
    {

        int currentLevel = GetSkillLevel(skillToBuy.skillID);
        if (currentLevel >= skillToBuy.maxNBuy)
        {
            StartCoroutine(messageError("L�mite alcanzado. Si mejoramos esto un nivel m�s, el motor de Unity explota y te borra el Windows."));
            audioSource.PlayOneShot(soundFailBuy);
            return;
        }

        int cost = skillToBuy.baseCost + (currentLevel * skillToBuy.costMultiplier);
        if (currentMeta.totalExperience < cost)
        {
            StartCoroutine(messageError("Sin experiencia no hay mejoras. Si las quieres gratis, haber comprado la edici�n Deluxe."));
            audioSource.PlayOneShot(soundFailBuy);
            return;
        }

        // --- �COMPRA ACEPTADA! ---

        // Descontamos la XP
        currentMeta.totalExperience -= cost;

        // Desbloqueamos la torre correspondiente en nuestro MetaSaveData
        UpLevel(skillToBuy.skillID);

        // Se aplica el efecto
        ApplyUpgrade(skillToBuy);
        audioSource.PlayOneShot(soundBuy);

        // Guardamos f�sicamente en el archivo JSON
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

        // Recorremos todos los botones y les decimos: "Oye, revisa si est�s bloqueado o comprado"
        foreach (SkillNode node in allNodesInTree)
        {
            // Pillamos el nivel actual de este nodo para pas�rselo a la UI
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

    private void InitTree()
    {
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

        // Comprobamos que el diccionario tiene la torre y que el �ndice es seguro (0, 1 o 2)
        if (currentMeta.upgradesTree.ContainsKey(towerName) && index >= 0 && index < currentMeta.upgradesTree[towerName].Count)
        {
            currentMeta.upgradesTree[towerName][index] += node.benefitPerLevel;
        }
        else
        {
            Debug.LogError($"[�rbol] Configuraci�n inv�lida en la carta {node.skillID}. �ndice {index} fuera de rango.");
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

    public void ResetTree()
    {
        foreach (SkillNode skill in allNodesInTree)
        {
            SkillProgress dataSkill = GetNode(skill.myData.skillID);
            
            // Comprobación de seguridad por si acaso el nodo no existía en el guardado
            if (dataSkill != null)
            {
                // Devolvemos la experiencia matemática
                currentMeta.totalExperience += (dataSkill.level * skill.myData.baseCost) + (skill.myData.costMultiplier * (dataSkill.level * (dataSkill.level - 1)) / 2);
                currentMeta.skillList.Remove(dataSkill);
            }
            
            // Reiniciamos el nodo a nivel 0
            InitNode(skill);
        }

        // Reseteamos bloqueos y mejoras
        currentMeta.isInfernalTowerUnlocked = false;
        currentMeta.isSupportTowerUnlocked = false;
        currentMeta.upgradesTree = new Dictionary<string, List<float>>
        {
            {"Torre Media", new List<float> { 1f, 1f, 1f } },
            {"Torre Ligera", new List<float> { 1f, 1f, 1f } },
            {"Torre Pesada", new List<float> { 1f, 1f, 1f } },
            {"Torre Infernal", new List<float> { 1f, 1f, 1f } },
            {"Torre Soporte", new List<float> { 1f, 1f, 1f } }
        };

        // --- LA CLAVE DEL ARREGLO ESTÁ AQUÍ ---
        // Guardamos forzosamente los datos reseteados en el archivo físico
        SaveSystem.SaveMeta(currentMeta);
        SaveSystem.DebugLogMetaSave(); // Opcional, para que lo veas en consola
        
        // Actualizamos visualmente
        UpdateUI();
    }
}
