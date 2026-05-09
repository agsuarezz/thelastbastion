using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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
    public static bool SaveExists()  => File.Exists(SavePath);

    public static void SaveMeta(MetaSaveData data)
    {
        try
        {
            File.WriteAllText(MetaSavePath, JsonUtility.ToJson(data, prettyPrint: true));
            Debug.Log($"[SaveSystem] MetaProgreso Guardado en: {MetaSavePath}");
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al guardar Meta: {e.Message}"); }
    }

    public static MetaSaveData LoadMeta()
    {
        if (!File.Exists(MetaSavePath)) return new MetaSaveData(); // Si no existe, devuelve uno nuevo en blanco
        try
        {
            return JsonUtility.FromJson<MetaSaveData>(File.ReadAllText(MetaSavePath));
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al cargar Meta: {e.Message}"); return new MetaSaveData(); }
    }
    public static void DebugLogMetaSave()
    {
        MetaSaveData data = LoadMeta();
        // LoadMeta siempre devuelve un objeto (aunque esté vacío), así que imprimimos siempre
        string json = JsonUtility.ToJson(data, true);
        Debug.Log($"<color=magenta>[SaveSystem] Contenido de METAPROGRESIÓN:</color>\n{json}");
    }
}