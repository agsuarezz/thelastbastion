using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Atributos")]
    public float speed = 8f;

    private castleScript targetCastle;
    private int damage;
    private bool hasHit;

    public void Launch(castleScript castle, int damageAmount)
    {
        targetCastle = castle;
        damage = damageAmount;
        hasHit = false;
    }

    private void Update()
    {
        if (targetCastle == null || !targetCastle.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardsCastle();
    }

    private void MoveTowardsCastle()
    {
        Vector3 targetPosition = GetTargetPosition();

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        RotateTowards(targetPosition);

        if (Vector2.Distance(transform.position, targetPosition) <= 0.05f)
        {
            HitCastle();
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (targetCastle.castleCollider != null)
        {
            return targetCastle.castleCollider.ClosestPoint(transform.position);
        }

        return targetCastle.transform.position;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude <= 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        castleScript castle = collision.GetComponent<castleScript>();

        if (castle != null && castle == targetCastle)
        {
            HitCastle();
        }
    }

    private void HitCastle()
    {
        if (hasHit) return;

        hasHit = true;

        if (targetCastle != null)
        {
            targetCastle.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}