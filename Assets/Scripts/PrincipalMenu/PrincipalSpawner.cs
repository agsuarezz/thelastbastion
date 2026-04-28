using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PrincipalEnemyTypeConfig
{
    public ObjectPooler pooler;
    public EnemyData enemyData;

    [Range(1, 100)]
    public int spawnWeight;

    [Tooltip("Marca esto solo para enemigos tipo Golem, para evitar demasiados seguidos.")]
    public bool isGolem;
}

public class PrincipalSpawner : MonoBehaviour
{
    [Header("Ruta del menú")]
    [SerializeField] private PrincipalRoute principalRoute;

    [Header("Tipos de enemigos")]
    [SerializeField] private PrincipalEnemyTypeConfig[] enemyTypes;

    [Header("Spawn aleatorio")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 4f;

    [Header("Límite en pantalla")]
    [SerializeField] private int maxEnemiesAlive = 6;

    [Header("Toque cinemático")]
    [SerializeField] private float initialDelay = 1.5f;
    [SerializeField] private int spawnsBeforeCinematicPause = 5;

    [Range(0f, 1f)]
    [SerializeField] private float cinematicPauseChance = 0.35f;

    [SerializeField] private float cinematicPauseMin = 4f;
    [SerializeField] private float cinematicPauseMax = 6f;

    [Header("Control de repetición")]
    [SerializeField] private int maxGolemsInARow = 2;

    private float spawnTimer;
    private int spawnCounter;

    private int golemsInARow = 0;

    private void Start()
    {
        spawnTimer = initialDelay;
        spawnCounter = 0;
        golemsInARow = 0;
    }

    private void Update()
    {
        if (!CanSpawn())
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnCounter++;
            SetNextSpawnTime();
        }
    }

    private bool CanSpawn()
    {
        if (principalRoute == null)
            return false;

        if (principalRoute.waypoints == null || principalRoute.waypoints.Length == 0)
            return false;

        if (enemyTypes == null || enemyTypes.Length == 0)
            return false;

        if (CountActiveEnemies() >= maxEnemiesAlive)
            return false;

        return true;
    }

    private void SetNextSpawnTime()
    {
        bool cinematicPause =
            spawnCounter > 0 &&
            spawnCounter % spawnsBeforeCinematicPause == 0 &&
            Random.value < cinematicPauseChance;

        spawnTimer = cinematicPause
            ? Random.Range(cinematicPauseMin, cinematicPauseMax)
            : Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnEnemy()
    {
        List<PrincipalEnemyTypeConfig> availableEnemies = GetAvailableEnemies();

        if (availableEnemies.Count == 0)
            return;

        PrincipalEnemyTypeConfig selectedEnemy = GetWeightedRandomEnemy(availableEnemies);

        GameObject enemyObject = selectedEnemy.pooler.GetPooledObject();

        if (enemyObject == null)
            return;

        enemyObject.transform.position = principalRoute.waypoints[0].position;
        enemyObject.transform.rotation = Quaternion.identity;

        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.enemyData = selectedEnemy.enemyData;
            enemy.SetPath(principalRoute.GetPositions());
        }

        enemyObject.SetActive(true);

        if (enemy != null)
        {
            enemy.SetLifeBarVisible(false);
        }

        UpdateGolemCounter(selectedEnemy);
    }

    private List<PrincipalEnemyTypeConfig> GetAvailableEnemies()
    {
        List<PrincipalEnemyTypeConfig> availableEnemies = new List<PrincipalEnemyTypeConfig>();

        foreach (PrincipalEnemyTypeConfig enemyConfig in enemyTypes)
        {
            if (enemyConfig.pooler == null)
                continue;

            if (enemyConfig.spawnWeight <= 0)
                continue;

            if (enemyConfig.isGolem && golemsInARow >= maxGolemsInARow)
                continue;

            availableEnemies.Add(enemyConfig);
        }

        return availableEnemies;
    }

    private void UpdateGolemCounter(PrincipalEnemyTypeConfig selectedEnemy)
    {
        if (selectedEnemy.isGolem)
        {
            golemsInARow++;
        }
        else
        {
            golemsInARow = 0;
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
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}