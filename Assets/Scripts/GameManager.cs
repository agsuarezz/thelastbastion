using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Controla la lógica general del nivel, la creación de defensas y el reinicio de la partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Imagen base para copiar el sprite y tamaño a las nuevas torres construidas.
    [Tooltip("Imagen base de la cual se copiará el sprite para las torres.")]
    public Image towerImage;
    /// <summary>
    /// Transforma una casilla vacía (botón) en una torre. 
    /// Le asigna el sprite correcto, ajusta su tamaño al de la imagen original y borra el texto.
    /// </summary>
    /// <param name="button">El GameObject del botón interactuado por el jugador.</param>
    public void buildTower(GameObject button)
    {
        if (!towerImage)
        {
            Debug.LogWarning("No se ha podido encontrar la imagen");
            return;
        }

        button.GetComponent<Image>().sprite = towerImage.sprite;
        RectTransform rectTorre = towerImage.GetComponent<RectTransform>();
        RectTransform rectBoton = button.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(rectTorre.rect.width, rectTorre.rect.height);
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