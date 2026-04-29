using UnityEngine;

/// <summary>
/// Script auxiliar que gestiona la mejora (upgrade) de una torre.
/// Detecta los clics del jugador para subir el nivel de la torre
/// y avisa al script principal para que aplique los cambios visuales y de estadísticas.
/// </summary>
public class UpdateTower : MonoBehaviour
{
    [Tooltip("Bandera que avisa al script principal (Tower.cs) que debe actualizar sus estadísticas o sprite.")]
    public bool needUpdateTower = false;

    [Tooltip("Nivel actual de la mejora de la torre. Empieza en 0.")]
    public int levelOfTower = 0;

    [Tooltip("Identificador del tipo de torre (ej. 0=Media, 1=Ligera, 2=Pesada). Un valor de -1 indica que no está asignado.")]
    public int typeOfTower = -1;
    /// <summary>
    /// Método que se enlaza al botón "Mejorar" de la interfaz de usuario.
    /// Valida si la torre puede mejorar (nivel máximo, tipo válido, coste) y, 
    /// de ser así, aumenta su nivel y avisa a la torre principal para que aplique los cambios.
    /// </summary>
    public void onClickPlayer()
    {

        // Comprobaciones de seguridad antes de aplicar la mejora:
        // 1. levelOfTower < 2: Asegura que no pase del nivel máximo (llegará hasta nivel 2).
        // 2. !needUpdateTower: Evita bugs si el jugador hace doble clic muy rápido (espera a que el script principal termine la mejora actual).
        // 3. typeOfTower != -1: Confirma que realmente hay una torre construida en esta casilla antes de intentar mejorarla.
        // 4. sprite != null
        int nextLevel = levelOfTower + 1;
        Tower tower = this.GetComponentInParent<Tower>();
        if (levelOfTower < 2 && !needUpdateTower && typeOfTower != -1 && GameManager.countMoney >= tower.config.upgradeCosts[nextLevel])
        {
            // Activamos la bandera para que el Tower.cs lo lea en su Update() y subimos el nivel
            Tower.setGameObjectUpDeleStatus(false);
            needUpdateTower = true;
        }
    }
  
}