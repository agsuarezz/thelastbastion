using UnityEngine;

/// <summary>
/// Componente que otorga a un enemigo la capacidad de atacar a distancia.
/// Responsabilidad única: detectar si el castillo está en rango y disparar proyectiles.
/// 
/// Uso: Añadir este componente al prefab del enemigo a distancia JUNTO a Enemy.cs.
/// Enemy.cs delega en este componente cuando llega al rango de ataque.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class RangedEnemyAttack : MonoBehaviour
{
    [Header("Proyectil")]
    [Tooltip("Prefab del proyectil que lanzará el enemigo.")]
    public GameObject projectilePrefab;

    [Tooltip("Punto de origen del disparo. Si es null, se usa la posición del enemigo.")]
    public Transform firePoint;

    // ── Estado interno ────────────────────────────────────────────────
    private float attackTimer = 0f;
    private castleScript targetCastle;
    private Enemy enemy; // Referencia para leer enemyData (daño, cooldown, rango)

    // ── Propiedad pública ────────────────────────────────────────────
    /// <summary>¿Está el castillo dentro del rango de ataque?</summary>
    public bool IsInRange => targetCastle != null && DistanceToCastle() <= enemy.enemyData.attackRange;

    // ── Unity lifecycle ──────────────────────────────────────────────
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        targetCastle = FindObjectOfType<castleScript>();
    }

    /// <summary>
    /// Debe ser llamado cada frame desde Enemy.cs cuando el enemigo está en modo ranged.
    /// </summary>
    public void Tick()
    {
        if (targetCastle == null)
        {
            targetCastle = FindObjectOfType<castleScript>();
            if (targetCastle == null) return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            FireProjectile();
            attackTimer = enemy.enemyData.attackCooldown;
        }
    }

    // ── Lógica privada ────────────────────────────────────────────────
    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[RangedEnemyAttack] {gameObject.name}: falta asignar el projectilePrefab.");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileGO = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        EnemyProjectile projectile = projectileGO.GetComponent<EnemyProjectile>();
        if (projectile == null)
        {
            Debug.LogError("[RangedEnemyAttack] El prefab no tiene el componente EnemyProjectile.");
            Destroy(projectileGO);
            return;
        }

        int damage = Mathf.RoundToInt(enemy.enemyData.damage * GameManager.globalEnemyDamageMultiplier);
        projectile.Launch(targetCastle.transform, damage);
    }

    private float DistanceToCastle()
    {
        if (targetCastle.castleCollider != null)
        {
            Vector2 closest = targetCastle.castleCollider.ClosestPoint(transform.position);
            return Vector2.Distance(transform.position, closest);
        }
        return Vector2.Distance(transform.position, targetCastle.transform.position);
    }

    /// <summary>Reinicia el temporizador al reactivar el objeto desde el pool.</summary>
    public void ResetTimer()
    {
        attackTimer = 0f;
    }
}