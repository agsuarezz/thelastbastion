using UnityEngine;
/// <summary>
/// Contiene la configuración específica de este mapa.
/// </summary>
public class CastleSpawnPoint : MonoBehaviour
{
    [Header("Posición del Castillo")]
    [Tooltip("Coordenada exacta donde debe aparecer el castillo en este mapa")]
    public Vector3 castleSpawnPosition;
    /// Propiedad que devuelve la posición real en el mundo, centrada en la casilla.
    /// (Suma +0.5f a X e Y de forma automática).
    /// </summary>
    public Vector3 CalculatedCenter => new Vector3(castleSpawnPosition.x, castleSpawnPosition.y + 0.5f, 0);
    private void OnDrawGizmos()
    {
        // Pintamos un cubo azul semitransparente para representar el castillo
        Gizmos.color = new Color(0, 0, 1, 0.5f); // Azul
        Gizmos.DrawCube(castleSpawnPosition, new Vector3(1f, 1f, 1f));

        // Y una línea roja pequeñita en el centro para marcar el punto exacto
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(castleSpawnPosition, 0.2f);
    }
}
