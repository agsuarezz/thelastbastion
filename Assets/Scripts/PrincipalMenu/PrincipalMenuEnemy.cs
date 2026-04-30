using UnityEngine;

public class PrincipalMenuEnemy : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;

    private void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PrincipalMenuProjectile>() != null)
        {
            Destroy(other.gameObject); // destruye proyectil
            Destroy(gameObject);       // destruye enemigo
        }
    }
}