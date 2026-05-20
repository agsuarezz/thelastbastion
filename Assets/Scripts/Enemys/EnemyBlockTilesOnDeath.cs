using UnityEngine;

public class EnemyBlockTilesOnDeath : MonoBehaviour
{
    [Header("Prefab de bloqueo")]
    public GameObject blockedTilePrefab;

    [Header("Configuración")]
    public int tilesToBlock = 5;
    public int roundsDuration = 3;
    public float blockRadius = 3f;

    [Header("Sonido")]
    public AudioClip fireSpawnSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    public void BlockTilesOnDeath()
    {
        if (fireSpawnSound != null)
{
    AudioSource.PlayClipAtPoint(
        fireSpawnSound,
        transform.position,
        soundVolume
    );
}
        int tilesBlocked = 0;
        int attempts = 0;

        while (tilesBlocked < tilesToBlock && attempts < 50)
        {
            attempts++;

            Vector2 randomOffset = Random.insideUnitCircle * blockRadius;

            float x = Mathf.Floor(transform.position.x + randomOffset.x) + 0.5f;
            float y = Mathf.Floor(transform.position.y + randomOffset.y) + 0.5f;

            Vector2 gridPosition = new Vector2(x, y);

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                gridPosition,
                new Vector2(0.1f, 0.1f),
                0f
            );

            bool canBlock = true;

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("tower") ||
                    hit.CompareTag("Path") ||
                    hit.CompareTag("Enemy") ||
                    hit.CompareTag("BlockedTile"))
                {
                    canBlock = false;
                    break;
                }
            }

            if (!canBlock)
                continue;

            GameObject block = Instantiate(
                blockedTilePrefab,
                gridPosition,
                Quaternion.identity
            );
            

            BlockedTile blockedTile = block.GetComponent<BlockedTile>();

            if (blockedTile != null)
                blockedTile.Init(roundsDuration);

            tilesBlocked++;
        }
    }
}