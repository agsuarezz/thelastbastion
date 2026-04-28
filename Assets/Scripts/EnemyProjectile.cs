using UnityEngine;

/// <summary>
/// Proyectil lanzado por un enemigo a distancia hacia el castillo.
/// Responsabilidad única: viajar hacia su objetivo y aplicar daño al impactar.
/// 
/// Espejo de Projectile.cs pero en la dirección contraria (enemigo → castillo).
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Atributos del Proyectil")]
    [Tooltip("Velocidad de vuelo.")]
    [HideInInspector] public float speed = 8f;

    // ── Estado interno ─────────────────────────────────────────────
    private Transform target;
    private int damage;
    private bool hasHit = false; // Evita doble impacto (Update + OnTrigger)

    // ── API pública ────────────────────────────────────────────────
    /// <summary>
    /// Inicializa el proyectil con destino y daño. Llamar justo tras Instantiate.
    /// </summary>
    public void Launch(Transform castleTransform, int damageAmount)
    {
        target = castleTransform;
        damage = damageAmount;
    }

    // ── Unity lifecycle ────────────────────────────────────────────
    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardsTarget();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("castle"))
        {
            HitCastle(collision.GetComponent<castleScript>());
        }
    }

    // ── Lógica privada ─────────────────────────────────────────────
    private void MoveTowardsTarget()
    {
        float step = speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        OrientTowardsTarget();

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            HitCastle(target.GetComponent<castleScript>());
        }
    }

    /// <summary>Rota el sprite para que apunte hacia el castillo durante el vuelo.</summary>
    private void OrientTowardsTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HitCastle(castleScript castle)
    {
        if (hasHit) return;
        hasHit = true;

        if (castle != null)
        {
            castle.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}