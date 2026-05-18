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
    [HideInInspector] public float damage = 20;

    // Lista de efectos al impacto (fuego, veneno, etc.). Vacía = sin efectos especiales.
    private readonly System.Collections.Generic.List<IOnHitEffect> _onHitEffects =
        new System.Collections.Generic.List<IOnHitEffect>();

    private Transform target;

    // ── API pública ──────────────────────────────────────────────────────────

    public void Seek(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// Añade un efecto al impacto. Se pueden registrar varios (fuego + veneno, etc.).
    /// </summary>
    public void AddOnHitEffect(IOnHitEffect effect)
    {
        if (effect != null)
            _onHitEffects.Add(effect);
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

            // Aplicamos todos los efectos registrados (quemadura, veneno, etc.)
            foreach (IOnHitEffect effect in _onHitEffects)
                effect.Apply(enemyScript);
        }

        Destroy(gameObject);
    }
}
