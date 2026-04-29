using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class RangedEnemyAttack : MonoBehaviour
{
    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Enemy enemy;
    private castleScript targetCastle;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        FindCastle();
    }

    // Animation Event para enemigos a distancia.
    public void Shoot()
    {
        if (enemy == null || enemy.IsDead) return;

        if (targetCastle == null)
        {
            FindCastle();
        }

        if (targetCastle == null) return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[RangedEnemyAttack] {gameObject.name}: falta asignar projectilePrefab.");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            Debug.LogError("[RangedEnemyAttack] El prefab no tiene EnemyProjectile.");
            Destroy(projectileObject);
            return;
        }

        int finalDamage = Mathf.RoundToInt(enemy.enemyData.damage * GameManager.globalEnemyDamageMultiplier);
        projectile.Launch(targetCastle, finalDamage);
    }

    private void FindCastle()
    {
        targetCastle = FindObjectOfType<castleScript>();
    }
}