using System.Collections;
using UnityEngine;

public class PrincipalMenuTower : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float range = 6f;

    private void Start()
    {
        StartCoroutine(ShootLoop());
    }

    private IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);

            PrincipalEnemy target = FindClosestEnemy();

            if (target != null)
            {
                Shoot(target.transform);
            }
        }
    }

    private PrincipalEnemy FindClosestEnemy()
    {
        PrincipalEnemy[] enemies = FindObjectsByType<PrincipalEnemy>(FindObjectsSortMode.None);

        PrincipalEnemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (PrincipalEnemy enemy in enemies)
        {
            if (!enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance && distance <= range)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private void Shoot(Transform target)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        PrincipalMenuProjectile projectile = projectileObject.GetComponent<PrincipalMenuProjectile>();

        if (projectile != null)
        {
            Vector3 direction = target.position - shootPoint.position;
            projectile.Init(direction);
        }
    }
}