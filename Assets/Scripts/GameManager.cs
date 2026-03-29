using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Controla la lógica general del nivel, la creación de defensas y el reinicio de la partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    // SpriteRenderer de referencia para asignar el gráfico correcto a las nuevas torres construidas.
    [Tooltip("SpriteRenderer base del cual se copiará el sprite para las torres.")]
    public SpriteRenderer towerImage;
    /// <summary>
    /// Construye una torre en la casilla seleccionada por el jugador. 
    /// Oculta la imagen del botón, busca el objeto torre asociado en la escena 
    /// y le asigna el sprite de referencia, además de borrar el texto del botón.
    /// </summary>
    /// <param name="button">El GameObject del botón interactuado por el jugador.</param>
    public void buildTower(GameObject button)
    {
        if (!towerImage)
        {
            Debug.LogWarning("No se ha podido encontrar la imagen");
            return;
        }
        string name = button.name + "_tower";
        button.GetComponent<Image>().enabled = false;
        GameObject.Find(name).GetComponent<SpriteRenderer>().sprite = towerImage.sprite;
        button.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }
    /// <summary>
    /// Quita la pausa del juego y recarga la escena inicial para empezar una nueva partida.
    /// </summary>
    public void restartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}