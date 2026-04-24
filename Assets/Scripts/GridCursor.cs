using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GridCursor : MonoBehaviour
{
    private LineRenderer line;

    [Tooltip("El tamaño de tu casilla. Normalmente es 1.")]
    public float cellSize = 1f;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        // Le decimos al LineRenderer que tendrá exactamente 4 esquinas
        line.positionCount = 4;
    }

    void Update()
    {
        // 1. Obtenemos la posición del ratón en el mundo real (2D)
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. Snapping: Redondeamos las coordenadas para saltar al centro perfecto de la casilla
        float x = Mathf.Floor(mousePosition.x) + 0.5f;
        float y = Mathf.Floor(mousePosition.y) + 0.5f;
        Vector2 cellCenter = new Vector2(x, y);

        // 3. Dibujamos el cuadrado alrededor de ese centro
        DrawSquare(cellCenter);

        // 4. Cambiamos el color si el escáner detecta que choca con un obstáculo
        UpdateDynamicColor(cellCenter);
    }

    void DrawSquare(Vector2 center)
    {
        float half = cellSize / 2f;

        // Calculamos las 4 esquinas exactas de la casilla
        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(center.x - half, center.y + half, -1f); // Arriba Izquierda
        points[1] = new Vector3(center.x + half, center.y + half, -1f); // Arriba Derecha
        points[2] = new Vector3(center.x + half, center.y - half, -1f); // Abajo Derecha
        points[3] = new Vector3(center.x - half, center.y - half, -1f); // Abajo Izquierda

        // Dibujamos las líneas conectando los puntos
        line.SetPositions(points);
    }

    void UpdateDynamicColor(Vector2 center)
    {
        // Lanzamos un escáner invisible para buscar obstáculos
        Collider2D hit = Physics2D.OverlapCircle(center, 0.2f);

        // Si tocamos algo, Y ese algo es una Torre, un Camino o un Enemigo...
        if (hit != null && (hit.CompareTag("tower") || hit.CompareTag("Path") || hit.CompareTag("Enemy")))
        {
            // Bloqueado: La línea se pone ROJA
            line.startColor = Color.red;
            line.endColor = Color.red;
        }
        else
        {
            // Libre: La línea se pone VERDE
            line.startColor = Color.green;
            line.endColor = Color.green;
        }
    }
}