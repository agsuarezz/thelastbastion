using Mono.Cecil;
using TMPro;
using Unity.VisualScripting;
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
    // Componente de audio compartido por todo el juego para emitir los sonidos.
    [Tooltip("AudioSource base del cual asignar sonidos.")]
    public static AudioSource audioSource;
    // Efecto de sonido que se reproduce al saltar el Game Over.
    public static AudioClip soundLostGame;
    // Efecto de sonido que se reproduce cuando un enemigo ataca y resta vida.
    public static AudioClip soundTakeLife;
    /// <summary>
    /// Método de inicialización. Vincula el componente AudioSource y carga los efectos 
    /// de sonido desde la carpeta 'Resources'. Emite advertencias en consola si falta algo.
    /// </summary>
    public void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        soundLostGame = Resources.Load<AudioClip>("soundLostGame");
        soundTakeLife = Resources.Load<AudioClip>("soundTakeLife");
        if (!audioSource)
        {
            Debug.LogWarning("No se ha encontrado audioSource");
        }
        if(!soundLostGame)
        {
            Debug.LogWarning("No se ha encontrado soundLostGame");
        }
        if (!soundTakeLife)
        {
            Debug.LogWarning("No se ha encontrado soundTakeLife");
        }
    }
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
        changeTimeScale();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /// <summary>
    /// Cambia el time.TimeScale
    /// Si es 0 a 1
    /// Si es 1 a 0
    /// </summary>
    public static void changeTimeScale()
    {
        Time.timeScale = Time.timeScale == 1.0f ? 0.0f : 1.0f;
    }
}