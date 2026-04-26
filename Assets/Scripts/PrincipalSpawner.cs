using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PrincipalEnemyTypeConfig
{
    [Tooltip("El Pooler que contiene los prefabs de este enemigo.")]
    public ObjectPooler pooler;

    [Tooltip("Los datos (vida, daño, velocidad) para este tipo de enemigo.")]
    public EnemyData enemyData;

    [Tooltip("Peso de aparición. Cuanto mayor sea, más probable será que aparezca.")]
    [Range(1, 100)]
    public int spawnWeight;
}

public class PrincipalSpawner : MonoBehaviour
{
    [Header("Ruta")]
    [Tooltip("Ruta que seguirán los enemigos del menú principal.")]
    private LevelRoute enemyRoute;

    [Header("Tipos de enemigos")]
    [Tooltip("Tipos de enemigos que pueden aparecer en el menú principal.")]
    [SerializeField] private PrincipalEnemyTypeConfig[] enemyTypes;

    [Header("Spawn aleatorio")]
    [Tooltip("Tiempo mínimo entre spawns normales.")]
    [SerializeField] private float minSpawnInterval = 1.2f;

    [Tooltip("Tiempo máximo entre spawns normales.")]
    [SerializeField] private float maxSpawnInterval = 2.8f;

    [Header("Límite en pantalla")]
    [Tooltip("Número máximo de enemigos activos al mismo tiempo.")]
    [SerializeField] private int maxEnemiesAlive = 6;

    [Header("Toque cinemático")]
    [Tooltip("Espera inicial antes de empezar a generar enemigos.")]
    [SerializeField] private float initialDelay = 1.5f;

    [Tooltip("Cada cuántos spawns puede aparecer una pausa cinematográfica.")]
    [SerializeField] private int spawnsBeforeCinematicPause = 5;

    [Tooltip("Probabilidad de que ocurra una pausa cinematográfica tras varios spawns.")]
    [Range(0f, 1f)]
    [SerializeField] private float cinematicPauseChance = 0.35f;

    [Tooltip("Duración mínima de la pausa cinematográfica.")]
    [SerializeField] private float cinematicPauseMin = 2.5f;

    [Tooltip("Duración máxima de la pausa cinematográfica.")]
    [SerializeField] private float cinematicPauseMax = 4.5f;

    private float spawnTimer;
    private int spawnCounter;
    private bool initialDelayFinished;

    private void Awake()
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogWarning("PrincipalSpawner: no hay enemigos asignados.");
        }
    }

    private void Start()
    {
        enemyRoute = FindAnyObjectByType<LevelRoute>();
        spawnTimer = initialDelay;
        initialDelayFinished = false;
        spawnCounter = 0;
    }

    private void Update()
    {
        if (!CanSpawn())
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnRandomEnemyByWeight();
            spawnCounter++;
            SetNextSpawnTime();
        }
    }

    private bool CanSpawn()
    {
        if (enemyRoute == null || enemyRoute.waypoints == null || enemyRoute.waypoints.Length == 0)
            return false;

        if (enemyTypes == null || enemyTypes.Length == 0)
            return false;

        if (CountActiveEnemies() >= maxEnemiesAlive)
            return false;

        return true;
    }

    private void SetNextSpawnTime()
    {
        if (!initialDelayFinished)
        {
            initialDelayFinished = true;
        }

        bool canDoCinematicPause = spawnCounter > 0 && spawnCounter % spawnsBeforeCinematicPause == 0;

        if (canDoCinematicPause && Random.value < cinematicPauseChance)
        {
            spawnTimer = Random.Range(cinematicPauseMin, cinematicPauseMax);
        }
        else
        {
            spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    private void SpawnRandomEnemyByWeight()
    {
        List<PrincipalEnemyTypeConfig> availableEnemies = new List<PrincipalEnemyTypeConfig>();

        foreach (PrincipalEnemyTypeConfig enemyConfig in enemyTypes)
        {
            if (enemyConfig.pooler != null && enemyConfig.spawnWeight > 0)
            {
                availableEnemies.Add(enemyConfig);
            }
        }

        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning("PrincipalSpawner: no hay poolers válidos asignados o los pesos son 0.");
            return;
        }

        PrincipalEnemyTypeConfig selectedEnemy = GetWeightedRandomEnemy(availableEnemies);

        GameObject obj = selectedEnemy.pooler.GetPooledObject();

        if (obj == null)
        {
            return;
        }

        obj.transform.position = enemyRoute.waypoints[0];
        obj.transform.rotation = Quaternion.identity;

        Enemy enemyComponent = obj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.enemyData = selectedEnemy.enemyData;
            enemyComponent.SetPath(enemyRoute.waypoints);
        }
        else
        {
            Debug.LogWarning("PrincipalSpawner: el objeto obtenido del pool no tiene script Enemy.");
        }

        obj.SetActive(true);

        // MUY IMPORTANTE:
        // lo ocultamos DESPUÉS de activar el objeto,
        // porque OnEnable() vuelve a encender la barra.
        if (enemyComponent != null)
        {
            enemyComponent.SetLifeBarVisible(false);
        }
    }

    private PrincipalEnemyTypeConfig GetWeightedRandomEnemy(List<PrincipalEnemyTypeConfig> enemies)
    {
        int totalWeight = 0;

        foreach (PrincipalEnemyTypeConfig enemyConfig in enemies)
        {
            totalWeight += enemyConfig.spawnWeight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        foreach (PrincipalEnemyTypeConfig enemyConfig in enemies)
        {
            accumulatedWeight += enemyConfig.spawnWeight;

            if (randomValue < accumulatedWeight)
            {
                return enemyConfig;
            }
        }

        return enemies[enemies.Count - 1];
    }

    private int CountActiveEnemies()
    {
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        return activeEnemies.Length;
    }
}