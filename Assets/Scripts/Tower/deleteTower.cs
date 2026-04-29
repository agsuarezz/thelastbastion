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
    /// Método que se enlaza al botón "Vender" de la interfaz de usuario.
    /// Oculta el menú global y activa la bandera (isDeleteTower) para que la torre 
    /// principal inicie su proceso de autodestrucción y reembolso.
    /// </summary>
    public void onClickPlayer()
    {
        Tower.setGameObjectUpDeleStatus(false);
        isDeleteTower = true;
        
    }
}
