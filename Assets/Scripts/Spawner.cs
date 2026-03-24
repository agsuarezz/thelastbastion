using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;

    private float spawnInterval = 1f;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            spawnTimer = spawnInterval;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        GameObject spawnedObject = Instantiate(prefab);
        spawnedObject.transform.position = transform.position;
    }
}