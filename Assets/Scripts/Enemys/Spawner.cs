using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyTypeConfig
{
    [Tooltip("El Pooler que contiene los prefabs de este enemigo.")]
    public ObjectPooler pooler;

    [Tooltip("Los datos (vida, daño, velocidad) para este tipo de enemigo.")]
    public EnemyData enemyData;

    [Tooltip("Cuántos puntos cuesta este enemigo.")]
    public int cost;

    [Tooltip("Ronda mínima a partir de la que puede aparecer.")]
    public int minRound;
}

public class Spawner : MonoBehaviour
{
    [Header("Ruta")]
    private LevelRoute enemyRoute;

    [Header("Tipos de Enemigos")]
    [SerializeField] private EnemyTypeConfig[] enemyTypes;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 1f;
    private float spawnTimer;

    [Header("Sistema de Oleadas")]
    public static int enemiesAlive = 0;

    private List<EnemyTypeConfig> currentWave = new List<EnemyTypeConfig>();
    private int currentWaveIndex = 0;

    private void Awake()
    {
        enemiesAlive = 0;

        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("No hay enemigos asignados al Spawner");
        }
    }

    private void Start()
    {
        enemyRoute = FindAnyObjectByType<LevelRoute>();
        PrepareWave();
    }

    private void Update()
    {
        if (GameManager.currentState != GameState.Playing) return;
        if (currentWaveIndex < currentWave.Count)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                SpawnEnemyFromWave();
            }
        }
    }

    private void PrepareWave()
    {
        int round = GameManager.countRound;
        int budget = GetWaveBudget(round);

        currentWave = GenerateWave(round, budget);
        currentWaveIndex = 0;
        spawnTimer = spawnInterval;

        Debug.Log($"Ronda {round} | Presupuesto: {budget} | Enemigos: {currentWave.Count}");
    }

    private int GetWaveBudget(int round)
    {
        return 5 + round * 3;
    }

    private List<EnemyTypeConfig> GenerateWave(int round, int budget)
    {
        List<EnemyTypeConfig> wave = new List<EnemyTypeConfig>();

        while (budget > 0)
        {
            List<EnemyTypeConfig> available = new List<EnemyTypeConfig>();

            foreach (var enemy in enemyTypes)
            {
                if (enemy.minRound <= round && enemy.cost <= budget)
                {
                    available.Add(enemy);
                }
            }

            if (available.Count == 0)
                break;

            EnemyTypeConfig selected = available[Random.Range(0, available.Count)];
            wave.Add(selected);
            budget -= selected.cost;
        }

        return wave;
    }

    private void SpawnEnemyFromWave()
    {
        if (enemyRoute == null || enemyRoute.waypoints.Length == 0)
        {
            Debug.LogWarning("Spawner sin ruta");
            return;
        }

        if (currentWaveIndex >= currentWave.Count)
            return;

        EnemyTypeConfig enemyConfig = currentWave[currentWaveIndex];

        if (enemyConfig.pooler == null)
        {
            Debug.LogWarning("Enemy sin pooler");
            currentWaveIndex++;
            return;
        }

        GameObject obj = enemyConfig.pooler.GetPooledObject();

        if (obj != null)
        {
            obj.transform.position = enemyRoute.waypoints[0];

            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.enemyData = enemyConfig.enemyData;
                enemy.SetPath(enemyRoute.waypoints);
            }

            obj.SetActive(true);

            currentWaveIndex++;
            enemiesAlive++;
        }
        else
        {
            Debug.LogWarning("¡El Pooler está vacío! El Spawner está esperando a que muera un enemigo.");
        }
    }

    public bool statusRound()
    {
        if (enemiesAlive <= 0 && currentWaveIndex >= currentWave.Count)
        {
            GameManager.countRound += 1;
            return true;
        }

        return false;
    }

    public void restartCountEnemy()
    {
        PrepareWave();
    }
}