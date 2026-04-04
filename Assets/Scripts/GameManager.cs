using Mono.Cecil;
using System.Collections;
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
    public TextMeshProUGUI messageErrorText;
    [Header("Control de Rondas")]
    [Tooltip("Texto de la interfaz que muestra el número de ronda actual.")]
    public TextMeshProUGUI messageRound;
    [Tooltip("Referencia al Spawner para poder consultarle el estado de la ronda.")]
    public Spawner spawner;
    // Contador global de la ronda en la que se encuentra el jugador.
    public static int countRound = 0;
    /// <summary>
    /// Método de inicialización. Vincula el componente AudioSource y carga los efectos 
    /// de sonido desde la carpeta 'Resources'. Emite advertencias en consola si falta algo.
    /// Añade al texto de Ronda el numero de la ronda
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
        messageRound.text = "Ronda " + countRound;
    }
    /// <summary>
    /// Se ejecuta antes que el Start. Ideal para limpiar variables estáticas 
    /// y asegurar que la ronda vuelva a 0 si el jugador reinicia la escena.
    /// </summary>
    private void Awake()
    {
        countRound = 0;
    }
    /// <summary>
    /// Comprueba en cada frame si el Spawner indica que la ronda ha finalizado.
    /// En caso afirmativo, prepara la siguiente oleada y actualiza el texto en pantalla.
    /// </summary>
    public void Update()
    {
        if (spawner.statusRound())
        {
            spawner.restartCountEnemy();
            messageRound.text = "Ronda " + countRound;
        }
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
    /// <summary>
    /// Muestra un mensaje de error en pantalla durante 2 segundos y luego lo borra.
    /// </summary>
    public IEnumerator messageError(string text)
    {
        messageErrorText.text = text;
        messageErrorText.color = Color.red;
        yield return new WaitForSeconds(2f);
        messageErrorText.text = "";
    }
}