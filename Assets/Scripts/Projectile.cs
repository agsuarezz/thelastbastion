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
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
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
        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
