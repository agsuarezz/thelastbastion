using UnityEngine;
/// <summary>
/// Script auxiliar que detecta los clics del ratón para indicar a la torre principal
/// que debe ser eliminada o vendida. 
/// Normalmente se coloca en un GameObject hijo que actúa como "botón" de venta.
/// </summary>
public class DeleteTower : MonoBehaviour
{
    [Tooltip("Bandera (flag) que avisa al script principal (Tower.cs) de que el jugador quiere borrar esta torre.")]
    public bool isDeleteTower = false;
    /// <summary>
    /// Método nativo de Unity que se ejecuta automáticamente cuando el jugador 
    /// hace clic izquierdo sobre el Collider2D de este GameObject.
    /// </summary>
    private void OnMouseDown()
    {
        isDeleteTower = true;
    }
}
