using UnityEngine;


/// <summary>
/// Gestiona la detección de objetivos mediante un radio configurable y dispara.
/// </summary>
public class Tower : MonoBehaviour
{
    [Header("Atributos de la torre.")]
    [Tooltip("Radio de detección de los enemigos.")]
    public float attackRadius = 3f;
    [Tooltip("Tiempo de recarga en segundos entre cada disparo.")]
    public float fireCooldown = 1f;

    [Header("Referencias.")]
    [Tooltip("El prefab del proyectil que se va a instanciar")]
    public GameObject projectilePrefab;
    
    private Transform currentTarget;
    private float fireTimer = 0f;

    private void Update()
    {
        Debug.Log("La torre está buscando...");
        UpdateTarget();
        if (currentTarget == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireCooldown;
        }
    }

    private void UpdateTarget()
    {
         Debug.Log("Me estoy ejecutando jijiji");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        // Nos chivamos de cuántos enemigos con ese Tag hay en pantalla
        Debug.Log(gameObject.name + " ve " + enemies.Length + " enemigos en el mapa.");

        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        // Si hay al menos un enemigo, nos dice a qué distancia exacta está el más cercano
        if (nearestEnemy != null)
        {
            Debug.Log("El enemigo más cercano está a una distancia de: " + shortestDistance + ". Mi radio es: " + attackRadius);
        }

        if (nearestEnemy != null && shortestDistance <= attackRadius)
        {
            currentTarget = nearestEnemy.transform;
            Debug.Log("¡OBJETIVO FIJADO! Preparando disparo...");
        }
        else
        {
            currentTarget = null;
        }
    }

    private void Shoot()
    {
        Vector3 startPos = transform.position;
        GameObject projectileGO = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(currentTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
