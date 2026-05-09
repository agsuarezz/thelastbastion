using UnityEngine;

public class LineDrawer: MonoBehaviour
{
    public LineRenderer lineRenderer;
    [SerializeField] private Transform currentTarget;
    private void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, currentTarget.position);
    }
}
