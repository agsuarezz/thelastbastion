using UnityEngine;

/// <summary>
/// Ruta visual sin necesidad de GameObjects hijos ni Prefabs.
/// Solo usa posiciones matemáticas (Vector3).
/// </summary>
public class LevelRoute : MonoBehaviour
{
    [Tooltip("Añade puntos aquí y escribe sus coordenadas (X, Y). Aparecerán en la escena.")]
    public Vector3[] waypoints;

    // Magia visual: Esto dibuja líneas y esferas en tu escena de Unity
    // para que puedas ver el camino sin tener que crear GameObjects reales.
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Dibujamos las esferas amarillas (Los puntos)
        Gizmos.color = Color.yellow;
        foreach (Vector3 punto in waypoints)
        {
            Gizmos.DrawSphere(punto, 0.2f);
        }

        // Dibujamos las líneas rojas que conectan los puntos (El camino)
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
        }
    }
}