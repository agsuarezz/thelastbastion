using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GridCursor : MonoBehaviour
{
    private LineRenderer line;

    [Tooltip("El tamaño de tu casilla. Normalmente es 1.")]
    public float cellSize = 1f;

    public LineRenderer lineRenderer;
    ConstructionMenu constructionMenu;
    int circleSegments = 50;
    void Start()
    {
        line = GetComponent<LineRenderer>();
        // Le decimos al LineRenderer que tendrá exactamente 4 esquinas
        constructionMenu = FindAnyObjectByType<ConstructionMenu>();
        if (constructionMenu == null)
            Debug.LogWarning("constructionMenu no encontrado");
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

        DrawRangeCircleInGame(cellCenter);
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
        // Obtenemos todos los objetos en la casilla
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(0.1f, 0.1f), 0f);
        bool hasObstacle = false;

        // Comprobamos si alguno de ellos es ilegal
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("tower") || hit.CompareTag("Path") || hit.CompareTag("Enemy"))
            {
                hasObstacle = true;
                break; // Bloqueado, salimos del bucle
            }
        }

        // Aplicamos el color
        if (hasObstacle)
        {
            line.startColor = Color.red;
            line.endColor = Color.red;
        }
        else
        {
            line.startColor = Color.green;
            line.endColor = Color.green;
        }

    }
    /// <summary>
    /// Dibuja de forma dinámica el radio de ataque de la torre que el jugador tiene seleccionada
    /// para construir. Mueve el círculo a la posición de la casilla actual (el ratón) y calcula
    /// los puntos matemáticamente para formar un anillo perfecto.
    /// </summary>
    /// <param name="center">La coordenada 2D exacta (centro de la casilla) donde está el ratón.</param>
    public void DrawRangeCircleInGame(Vector2 center)
    {
        if (lineRenderer == null) return;

        // Extraemos el radio base del prefab y le aplicamos los multiplicadores globales de la partida
        float realRadius = setRange() * GameManager.globalRadiusMultiplier;

        // Le decimos al LineRenderer cuántos puntos va a tener la línea
        lineRenderer.positionCount = circleSegments;
        lineRenderer.useWorldSpace = false; // Para que siga al ratón, no al mundo
        lineRenderer.loop = true; // Cierra el círculo perfectamente
        lineRenderer.transform.position = new Vector3(center.x, center.y, 0f);

        // Matemáticas para dibujar un círculo punto por punto
        float angle = 0f;
        for (int i = 0; i < circleSegments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * realRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * realRadius;

            // PONEMOS LA Z EN -1 PARA QUE NO SE ESCONDA DETRÁS DEL CÉSPED
            lineRenderer.SetPosition(i, new Vector3(x, y, -1f));

            angle += (360f / circleSegments);
        }
    }

    /// <summary>
    /// Consulta el menú de construcción para saber qué tipo de torre está intentando poner el jugador.
    /// Lee el attackRadius del Prefab. Si está a 0 (no inicializado), usa el config por defecto.
    /// </summary>
    /// <returns>El radio de ataque en formato float de la torre seleccionada.</returns>
    public float setRange()
    {
        Tower towerPrefab = null;

        switch (constructionMenu.flagTypeTower)
        {
            case 0:
                towerPrefab = constructionMenu.prefabTowerMedian.GetComponent<Tower>();
                break;
            case 1:
                towerPrefab = constructionMenu.prefabTowerLight.GetComponent<Tower>();
                break;
            case 2:
                towerPrefab = constructionMenu.prefabTowerHeavy.GetComponent<Tower>();
                break;
            case 3:
                towerPrefab = constructionMenu.prefabTowerInfernal.GetComponent<Tower>();
                break;
            default:
                Debug.LogWarning("¡Cuidado! No se ha encontrado el attackRadius porque el flagTypeTower no es válido.");
                return 9999;
        }

        if (towerPrefab != null && towerPrefab.config != null)
        {
            return towerPrefab.config.baseAttackRadius;
        }

        // Valor de seguridad por si todo explota
        return 3f;
    }

}
