using UnityEngine;

/// <summary>
/// Almacena en orden los puntos (waypoints) que forman el camino del nivel.
/// </summary>
public class LevelRoute : MonoBehaviour
{
    [Tooltip("Arrastra aquí los puntos del mapa en orden")]
    public Transform[] waypoints;
}
