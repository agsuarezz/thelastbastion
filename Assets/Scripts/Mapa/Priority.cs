using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class Priority : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Ajustes")]
    public int baseOrder = 10000;
    public int multiplier = 100;
    public int minOrder = 0;
    public int maxOrder = 20000;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (sr == null) return;

        float yPos = transform.position.y - (sr.bounds.size.y / 2f);

        int order = baseOrder - Mathf.RoundToInt(yPos * multiplier);

        sr.sortingOrder = Mathf.Clamp(order, minOrder, maxOrder);
    }
}