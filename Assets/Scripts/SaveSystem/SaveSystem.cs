using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// ═════════════════════════════════════════════════════════════════════════════
//  DATOS SERIALIZABLES
// ═════════════════════════════════════════════════════════════════════════════

[Serializable]
public class TowerSaveData
{
    public float posX;
    public float posY;
    public int   towerType;        // 0=Mediana 1=Ligera 2=Pesada 3=Infernal
    public int   level;            // 0, 1 o 2
    public int   totalGoldInvested;
}
// ═════════════════════════════════════════════════════════════════════════════
//  DATOS SERIALIZABLES PARA METAPROGRESIÓN (ÁRBOL DE MEJORAS)
// ═════════════════════════════════════════════════════════════════════════════
[Serializable]
public class MetaSaveData
{
    public int totalExperience; // La XP global para gastar en el árbol

    public bool isInfernalTowerUnlocked = false;
    public bool isSupportTowerUnlocked = false;
    public List<SkillProgress> skillList = new List<SkillProgress>();
    public Dictionary<string, List<float>> upgradesTree = new Dictionary<string, List<float>>
    {
        {"Torre Media", new List<float> { 1f, 1f, 1f } },
        {"Torre Ligera", new List<float> { 1f, 1f, 1f } },
        {"Torre Pesada", new List<float> { 1f, 1f, 1f } },
        {"Torre Infernal", new List<float> { 1f, 1f, 1f } },
        {"Torre Soporte", new List<float> { 1f, 1f, 1f } }
    };
}
// ═════════════════════════════════════════════════════════════════════════════
//  DATOS DE LOS NODOS (ÁRBOL DE MEJORAS)
// ═════════════════════════════════════════════════════════════════════════════
[Serializable]
public class SkillProgress
{
    public string id;
    public int level;
    public bool buyed;
}

[Serializable]
public class GameSaveData
{
    // ── Progreso ──────────────────────────────────────────────────────────
    public int   countRound;
    public int   countMoney;
    public float timeinGame;
    public int countEnemiesDied;
    public int   countTower;
    public int enemiesDestroyed;
    // ── Boss ──────────────────────────────────────────────────────────
    public bool hasBossAppeared;

    // ── Castillo ──────────────────────────────────────────────────────────
    public int castleLife;
    public int castleLifeMax;

    // ── Multiplicadores globales ──────────────────────────────────────────
    public int   globalMoneyMultiplier;
    public float globalCostMultiplier;
    public float globalDamageTakenMultiplier;
    public float globalAttackSpeedMultiplier;
    public float globalSpeedMultiplier;
    public float globalRadiusMultiplier;
    public float globalEnemyHealthMultiplier;
    public float globalEnemyDamageMultiplier;
    public int gridIndex = -1;

    // ── Bonificaciones de cartas (globales) ───────────────────────────────
    public float globalMoneyBonusMultiplier;
    public float globalBurnProbability;
    public float globalPoisonProbability;
    public float globalChainLightningChance;
    public float globalSlowChance;

    // ── Bonificaciones de cartas por tipo de torre ────────────────────────
    // Serializamos como listas paralelas de clave/valor porque
    // JsonUtility no soporta Dictionary directamente.
    public List<string> towerDamageKeys      = new List<string>();
    public List<float>  towerDamageValues    = new List<float>();
    public List<string> towerSpeedKeys       = new List<string>();
    public List<float>  towerSpeedValues     = new List<float>();
    public List<string> towerRadiusKeys      = new List<string>();
    public List<float>  towerRadiusValues    = new List<float>();
    public List<string> towerBurnKeys        = new List<string>();
    public List<float>  towerBurnValues      = new List<float>();
    public List<string> towerPoisonKeys      = new List<string>();
    public List<float>  towerPoisonValues    = new List<float>();
    public List<string> towerChainKeys       = new List<string>();
    public List<float>  towerChainValues     = new List<float>();
    public List<string> towerSlowKeys        = new List<string>();
    public List<float>  towerSlowValues      = new List<float>();

    // ── Torres ────────────────────────────────────────────────────────────
    public List<TowerSaveData> towers = new List<TowerSaveData>();

    // ── Metadatos ─────────────────────────────────────────────────────────
    public string saveDate;
}

// ═════════════════════════════════════════════════════════════════════════════
//  SAVE SYSTEM  —  lógica pura de lectura/escritura
// ═════════════════════════════════════════════════════════════════════════════
public static class SaveSystem
{
    private static readonly string SavePath =
        Path.Combine(Application.persistentDataPath, "savegame.json");

    //Ruta en donde se guarda SERIALIZABLES PARA METAPROGRESIÓN (ÁRBOL DE MEJORAS)
    private static readonly string MetaSavePath = Path.Combine(Application.persistentDataPath, "metaprogression.json");
    public static void Save(GameSaveData data)
    {
        try
        {
            data.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
            Debug.Log($"[SaveSystem] Guardado en: {SavePath}");
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al guardar: {e.Message}"); }
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath)) return new GameSaveData();
        try
        {
            return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));
        }
        catch (Exception e) { return new GameSaveData(); }
    }
    public static void DeleteSave() { if (File.Exists(SavePath)) File.Delete(SavePath); }
    public static void DeleteMeta() { if (File.Exists(MetaSavePath)) File.Delete(MetaSavePath); }
    public static bool SaveExists()  => File.Exists(SavePath);

    // ── Helpers de serialización de diccionarios ──────────────────────────

    /// <summary>Vuelca un Dictionary&lt;string,float&gt; en dos listas paralelas dentro de GameSaveData.</summary>
    public static void PackDict(Dictionary<string, float> dict, List<string> keys, List<float> values)
    {
        keys.Clear();
        values.Clear();
        foreach (var kv in dict) { keys.Add(kv.Key); values.Add(kv.Value); }
    }

    /// <summary>Reconstruye un Dictionary&lt;string,float&gt; desde las listas paralelas guardadas.</summary>
    public static Dictionary<string, float> UnpackDict(List<string> keys, List<float> values)
    {
        var dict = new Dictionary<string, float>();
        if (keys == null || values == null) return dict;
        int count = Mathf.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++) dict[keys[i]] = values[i];
        return dict;
    }

    public static void SaveMeta(MetaSaveData data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(MetaSavePath, json);
            Debug.Log($"[SaveSystem] MetaProgreso Guardado en: {MetaSavePath}");
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al guardar Meta: {e.Message}"); }
    }

    public static MetaSaveData LoadMeta()
    {
        if (!File.Exists(MetaSavePath)) return new MetaSaveData(); // Si no existe, devuelve uno nuevo en blanco
        try
        {
            string json = File.ReadAllText(MetaSavePath);
            MetaSaveData data = JsonConvert.DeserializeObject<MetaSaveData>(json);
            if (data.upgradesTree == null)
            {
                data.upgradesTree = new Dictionary<string, List<float>>
                {
                    {"Torre Media", new List<float> { 1f, 1f, 1f } },
                    {"Torre Ligera", new List<float> { 1f, 1f, 1f } },
                    {"Torre Pesada", new List<float> { 1f, 1f, 1f } },
                    {"Torre Infernal", new List<float> { 1f, 1f, 1f } },
                    {"Torre Soporte", new List<float> { 1f, 1f, 1f } }
                };
            }
            return data;
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al cargar Meta: {e.Message}"); return new MetaSaveData(); }
    }
    public static void DebugLogMetaSave()
    {
        MetaSaveData data = LoadMeta();
        // LoadMeta siempre devuelve un objeto (aunque esté vacío), así que imprimimos siempre
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        Debug.Log($"<color=magenta>[SaveSystem] Contenido de METAPROGRESIÓN:</color>\n{json}");
    }
}