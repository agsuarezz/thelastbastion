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

[Serializable]
public class GameSaveData
{
    // ── Progreso ──────────────────────────────────────────────────────────
    public int   countRound;
    public int   countMoney;
    public float timeinGame;
    public int   enemiesDestroyed;
    public int   countTower;

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
        if (!File.Exists(SavePath)) return null;
        try
        {
            var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));
            Debug.Log($"[SaveSystem] Cargado (guardado el {data.saveDate})");
            return data;
        }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Error al cargar: {e.Message}"); return null; }
    }

    public static void DeleteSave() { if (File.Exists(SavePath)) File.Delete(SavePath); }
    public static bool SaveExists()  => File.Exists(SavePath);
}