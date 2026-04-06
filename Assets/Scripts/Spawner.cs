using UnityEngine;
/// <summary>
/// Se encarga de instanciar de forma periódica un objeto (enemigo) en la posición actual.
/// </summary>

public class Spawner : MonoBehaviour
{
    [Tooltip("El prefab del enemigo que se va a instanciar en la escena.")]
    public GameObject prefab;
    [Tooltip("La ruta que seguirán los enemigos generados por este Spawner.")]
    public LevelRoute enemyRoute;
    // Tiempo en segundos que debe transcurrir entre la aparición de cada enemigo.
    private float spawnInterval = 1f;
    // Temporizador interno para llevar la cuenta regresiva hasta el próximo spawn.
    private float spawnTimer;

    [Header("Sistema de Oleadas")]
    // Control global de enemigos vivos o activos en la escena.
    public static int enemiesAlive = 0;

    // Contador interno de los enemigos que YA han salido en la ronda actual.
    int enemiesSpawned = 0;

    // Cantidad límite de enemigos que el Spawner debe soltar en esta ronda.
    int countMaxEnemy = 5;
    public void Start()
    {
        Debug.Log("enemiesAlive" + enemiesAlive);
        Debug.Log("enemiesSpawned" + enemiesSpawned);
    }
    /// <summary>
    /// Reduce el temporizador frame a frame. Cuando llega a cero o menos, 
    /// reinicia el contador y llama a la función para crear el enemigo.
    /// </summary>
    private void Update()
    {
        Debug.Log("enemiesAlive " + enemiesAlive);
        Debug.Log("enemiesSpawned " + enemiesSpawned);
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
    /// <summary>
    /// Crea una nueva instancia del prefab y ajusta su posición para que 
    /// coincida exactamente con la posición de este Spawner.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyRoute == null || enemyRoute.waypoints.Length == 0)
        {
            Debug.LogWarning("El Spawner no tiene una ruta asignada.");
            return;
        }
        GameObject spawnedObject = Instantiate(prefab);
        spawnedObject.transform.position = enemyRoute.waypoints[0].position;

        Enemy enemyScript = spawnedObject.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetPath(enemyRoute.waypoints);
        }
        enemiesSpawned++;
        enemiesAlive++;
    }
    /// <summary>
    /// Limpiamos la memoria de los enemigos vivos por si venimos de un reinicio de escena.
    /// Evita el bug de arrastrar enemigos "fantasmas" de partidas anteriores.
    /// </summary>
    private void Awake()
    {
        enemiesAlive = 0;
    }
    /// <summary>
    /// Comprueba si la ronda actual ha terminado.
    /// Una ronda se considera terminada cuando han salido todos los enemigos límite y ya no queda ninguno vivo.
    /// </summary>
    /// <returns>True si la ronda terminó, False en caso contrario.</returns>
    public bool statusRound()
    {
        if (enemiesAlive <= 0 && enemiesSpawned == countMaxEnemy)
        {
            GameManager.countRound += 1;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Prepara el spawner para la siguiente oleada, reiniciando los contadores 
    /// y subiendo la dificultad (añadiendo enemigos extra al límite).
    /// </summary>
    public void restartCountEnemy()
    {
        enemiesSpawned = 0;
        countMaxEnemy += 1;
        spawnTimer = spawnInterval;
    }


}