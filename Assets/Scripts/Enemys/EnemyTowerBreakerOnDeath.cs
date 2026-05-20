using UnityEngine;

public class EnemyTowerBreakerOnDeath : MonoBehaviour
{
    [Header("Configuración")]
    public float breakRadius = 2.5f;
    public int maxTowersToBreak = 1;

    [Header("Efecto visual")]
    public GameObject towerDestroyEffectPrefab;
    public float destroyDelay = 0.6f;

    private Enemy enemy;
    private bool hasTriggered = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        hasTriggered = false;

        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (enemy != null)
            enemy.OnEnemyDied += BreakTowersOnDeath;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.OnEnemyDied -= BreakTowersOnDeath;
    }

    private void BreakTowersOnDeath(Enemy deadEnemy)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, breakRadius);

        int towersBroken = 0;

        foreach (Collider2D hit in hits)
        {
            Tower tower = hit.GetComponentInParent<Tower>();

            if (tower == null) continue;
            if (!tower.isBuilt) continue;

            tower.DestroyByEnemy(towerDestroyEffectPrefab, destroyDelay);

            towersBroken++;

            if (towersBroken >= maxTowersToBreak)
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breakRadius);
    }
}