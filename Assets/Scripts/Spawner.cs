using UnityEngine;

// 1. Creamos una estructura para vincular cada Pooler con sus Datos en el Inspector
[System.Serializable]
public struct EnemyTypeConfig
{
    [Tooltip("El Pooler que contiene los prefabs de este enemigo.")]
    public ObjectPooler pooler;
    [Tooltip("Los datos (vida, daño, velocidad) para este tipo de enemigo.")]
    public EnemyData enemyData;
}

/// <summary>
/// Se encarga de gestionar el ritmo de aparición y las oleadas, 
/// eligiendo aleatoriamente entre varios tipos de enemigos.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Ruta")]
    [Tooltip("La ruta que seguirán los enemigos generados por este Spawner.")]
    public LevelRoute enemyRoute;
    
    [Header("Tipos de Enemigos Disponibles")]
    [Tooltip("Añade aquí los diferentes poolers y sus datos asociados.")]
    [SerializeField] private EnemyTypeConfig[] enemyTypes;

    // Tiempo en segundos que debe transcurrir entre la aparición de cada enemigo.
    private float spawnInterval = 1f;
    // Temporizador interno para llevar la cuenta regresiva hasta el próximo spawn.
    private float spawnTimer;

    [Header("Sistema de Oleadas")]
    public static int enemiesAlive = 0;
    int enemiesSpawned = 0;
    int countMaxEnemy = 5;

    private void Awake()
    {
        enemiesAlive = 0;
        
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("¡ATENCIÓN! No has asignado ningún tipo de enemigo al Spawner: " + gameObject.name);
        }
    }
    
    private void Update()
    {
        if(enemiesSpawned < countMaxEnemy)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                spawnTimer = spawnInterval;
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyRoute == null || enemyRoute.waypoints.Length == 0)
        {
            Debug.LogWarning("El Spawner no tiene una ruta asignada.");
            return;
        }

        if (enemyTypes == null || enemyTypes.Length == 0) return;

        // 2. Elegimos un enemigo de forma totalmente aleatoria
        int randomIndex = Random.Range(0, enemyTypes.Length);
        EnemyTypeConfig chosenEnemy = enemyTypes[randomIndex];

        // 3. Verificamos que el pooler de ese enemigo esté asignado
        if (chosenEnemy.pooler == null) 
        {
            Debug.LogWarning("Falta un pooler en el índice " + randomIndex);
            return;
        }

        // 4. Pedimos el objeto al pooler elegido
        GameObject spawnedObject = chosenEnemy.pooler.GetPooledObject();
        
        if (spawnedObject != null)
        {
            spawnedObject.transform.position = enemyRoute.waypoints[0].position;
            Enemy enemyScript = spawnedObject.GetComponent<Enemy>();
            
            if (enemyScript != null)
            {
                enemyScript.SetPath(enemyRoute.waypoints);
                // Le inyectamos los datos específicos del enemigo que ha tocado
                enemyScript.enemyData = chosenEnemy.enemyData;
            }

            spawnedObject.SetActive(true);
            enemiesSpawned++;
            enemiesAlive++;
        }
    }

    public bool statusRound()
    {
        if (enemiesAlive <= 0 && enemiesSpawned == countMaxEnemy)
        {
            GameManager.countRound += 1;
            return true;
        }
        return false;
    }

    public void restartCountEnemy()
    {
        enemiesSpawned = 0;
        countMaxEnemy += 1;
        spawnTimer = spawnInterval;
    }
}