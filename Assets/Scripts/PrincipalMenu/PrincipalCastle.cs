using UnityEngine;

public class PrincipalCastle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PrincipalEnemy enemy = other.GetComponent<PrincipalEnemy>();

        if (enemy != null)
        {
            enemy.DisableEnemy();
        }
    }
}