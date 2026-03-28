using UnityEngine;
/// <summary>
/// Se encarga de instanciar de forma periódica un objeto (enemigo) en la posición actual.
/// </summary>

public class Spawner : MonoBehaviour
{
    [Tooltip("El prefab del enemigo que se va a instanciar en la escena.")]
    public GameObject prefab;
    // Tiempo en segundos que debe transcurrir entre la aparición de cada enemigo.
    private float spawnInterval = 1f;
    // Temporizador interno para llevar la cuenta regresiva hasta el próximo spawn.
    private float spawnTimer;
    /// <summary>
    /// Reduce el temporizador frame a frame. Cuando llega a cero o menos, 
    /// reinicia el contador y llama a la función para crear el enemigo.
    /// </summary>
    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            spawnTimer = spawnInterval;
            SpawnEnemy();
        }
    }
    /// <summary>
    /// Crea una nueva instancia del prefab y ajusta su posición para que 
    /// coincida exactamente con la posición de este Spawner.
    /// </summary>
    private void SpawnEnemy()
    {
        GameObject spawnedObject = Instantiate(prefab);
        spawnedObject.transform.position = transform.position;
    }
    
}