using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Controla la lógica general del nivel, la creación de defensas y el reinicio de la partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    public Image imagenTorre;
    /// <summary>
    /// Transforma una casilla vacía (botón) en una torre. 
    /// Le asigna el sprite correcto, ajusta su tamaño al de la imagen original y borra el texto.
    /// </summary>
    /// <param name="boton">El GameObject del botón interactuado por el jugador.</param>
    public void crearTorre(GameObject boton)
    {
        boton.GetComponent<Image>().sprite = imagenTorre.sprite;
        RectTransform rectTorre = imagenTorre.GetComponent<RectTransform>();
        RectTransform rectBoton = boton.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(rectTorre.rect.width, rectTorre.rect.height);
        boton.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }
    /// <summary>
    /// Quita la pausa del juego y recarga la escena inicial para empezar una nueva partida.
    /// </summary>
    public void restablecerJuego()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}