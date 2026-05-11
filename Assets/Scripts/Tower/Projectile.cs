using UnityEngine;

/// <summary>
/// Controla el viaje del disparo hacia el objetivo y aplica el daño al colisionar con el enemigo.
/// Admite un efecto opcional al impacto (IOnHitEffect) para poder añadir quemaduras, veneno,
/// ralentización, etc. sin modificar esta clase (Principio Abierto/Cerrado).
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Atributos del Proyectil")]
    [Tooltip("Velocidad de vuelo del proyectil")]
    [HideInInspector] public float speed = 20f;

    [Tooltip("Cantidad de vida que restará al impactar.")]
    [HideInInspector] public int damage = 20;

    // Efecto opcional que se ejecuta al impactar (null = sin efecto especial)
    private IOnHitEffect _onHitEffect;

    private Transform target;

    // ── API pública ──────────────────────────────────────────────────────────

    public void Seek(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Asigna un efecto al impacto. Pasar <c>null</c> elimina cualquier efecto previo.
    /// </summary>
    public void SetOnHitEffect(IOnHitEffect effect)
    {
        _onHitEffect = effect;
    }

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (EnemyTimeStopAbility.IsTimeStopped) return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        float distanceThisFrame = speed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            distanceThisFrame
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            HitTarget(target.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && target != null && collision.gameObject == target.gameObject)
        {
            HitTarget(collision.gameObject);
        }
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private void HitTarget(GameObject enemyGO)
    {
        if (enemyGO == null || !enemyGO.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            if (enemyScript.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            enemyScript.TakeDamage(damage);

            // Aplicamos el efecto especial si existe (quemadura, veneno, etc.)
            _onHitEffect?.Apply(enemyScript);
        }

        Destroy(gameObject);
    }
}
