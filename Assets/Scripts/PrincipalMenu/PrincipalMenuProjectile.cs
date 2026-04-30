using UnityEngine;

public class PrincipalMenuProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PrincipalEnemy enemy = other.GetComponent<PrincipalEnemy>();

        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
        }
    }
}