using UnityEngine;

/// <summary>
/// Controla el viaje del disparo hacia el objetivo y aplica el daño al colisionar con el enemigo
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Atributos del Proyectil")]
    [Tooltip("Velocidad de vuelo del proyectil")]
    public float speed = 15f;

    [Tooltip("Cantidad de vida que restará al impactar.")]
    public int damage = 25;

    private Transform target;

    public void Seek(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
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
        if (collision.CompareTag("Enemy"))
        {
            HitTarget(collision.gameObject);
        }
    }

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
    }

    Destroy(gameObject);
}
}