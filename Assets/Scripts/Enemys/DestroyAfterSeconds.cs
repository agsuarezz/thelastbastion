using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float time = 1f;

    private void Start()
    {
        Destroy(gameObject, time);
    }
}