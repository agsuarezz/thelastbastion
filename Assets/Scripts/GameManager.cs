using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// Las de VisualScripting y UnityEditor han sido eliminadas

/// <summary>
/// Controla la lógica general del nivel, la creación de defensas y el reinicio de la partida.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Ajustes de Sonido")]
    public GameObject botonMusica;
    public Image imagenBotonMusica;
    public Sprite iconoMusicaOn;
    public Sprite iconoMusicaOff;
    private bool musicaActiva = true;

    // SpriteRenderer de referencia para asignar el gráfico correcto a las nuevas torres construidas.
    [Tooltip("SpriteRenderer base del cual se copiará el sprite para las torres.")]
    public SpriteRenderer towerImage;
    // Componente de audio compartido por todo el juego para emitir los sonidos.
    [Tooltip("AudioSource base del cual asignar sonidos.")]
    public static AudioSource audioSource;
    // Efecto de sonido que se reproduce al saltar el Game Over.
    public static AudioClip soundLostGame;
    // Efecto de sonido que se reproduce al pausar.
    public static AudioClip soundPause;
    // Efecto de sonido que se reproduce al Restart.
    public static AudioClip soundRestart;
    // Efecto de sonido que se reproduce cuando un enemigo ataca y resta vida.
    public static AudioClip soundTakeLife;
    public TextMeshProUGUI messageErrorText;
    [Header("Control de Rondas")]
    [Tooltip("Texto de la interfaz que muestra el número de ronda actual.")]
    public TextMeshProUGUI messageRound;
    [Tooltip("Referencia al Spawner para poder consultarle el estado de la ronda.")]
    public Spawner spawner;
    [Tooltip("Referencia del castillo.")]
    public castleScript castlescript;
    // Contador global de la ronda en la que se encuentra el jugador.
    public static int countRound = 0;
    // Contador global del tiempo transcurrido en segundos desde que empezó la partida.
    // Se utiliza para mostrar cuánto aguantó el jugador en la pantalla de derrota.
    public static float timeinGame;
    // Registro total de enemigos eliminados por las defensas del jugador durante la partida.
    public static int enemiesDestroyed = 0;
    // Cantidad total de torres que el jugador ha construido en el mapa a lo largo del juego.
    public static int countTower = 0;
    [Header("Economía y Eventos")]

    /// <summary>
    /// Cantidad actual de monedas que posee el jugador para construir y mejorar torres. 
    /// Al ser estática, permite que cualquier script (como los enemigos al ser destruidos) pueda modificarla fácilmente.
    /// </summary>
    public static int countMoney = 0;
    /// <summary>
    /// Referencia al componente de texto de la interfaz gráfica (UI) encargado de mostrar el oro disponible en pantalla.
    /// </summary>
    public TextMeshProUGUI countMoneyText;
    /// <summary>
    /// Enlace al script gestor de eventos aleatorios, utilizado para disparar eventos (como estampidas o bonus) desde aquí.
    /// </summary>
    public randomEvents randomEvent;
    /// <summary>
    /// Multiplicador global que afecta a todo el oro ganado durante la partida. 
    /// Su valor por defecto es 1. Se altera temporalmente durante eventos especiales (ej. Frenesí Capitalista).
    /// </summary>
    public static int globalMoneyMultiplier = 1;
    /// <summary>
    /// Multiplicador global que afecta al precio de la torre. 
    /// Su valor por defecto es 1. Se altera temporalmente durante eventos especiales.
    /// </summary>
    public static float globalCostMultiplier = 1f;
    /// <summary>
    /// Multiplicador global que afecta a la vida que quita al enemigo. 
    /// Su valor por defecto es 1. Se altera temporalmente durante eventos especiales.
    /// </summary>
    public static float globalDamageTakenMultiplier = 1f;
    /// <summary>
    /// Multiplicador global que afecta al cooldown de disparo 
    /// Su valor por defecto es 1. Se altera temporalmente durante eventos especiales.
    /// </summary>
    public static float globalAttackSpeedMultiplier = 1f;
    // Se utiliza en randomEvents para que los enemigos vaya mas rapido
    public static float globalSpeedMultiplier = 1f;
    public static float globalRadiusMultiplier = 1f;
    public static float globalEnemyHealthMultiplier = 1f;
    public static float globalEnemyDamageMultiplier = 1f;

    [Header("Sistema de Cartas")]
    public CardManager cardManager;
    [Tooltip("Cada cuántas rondas aparecerán las cartas.")]
    public int roundsForCards = 1;

    /// <summary>
    /// Método de inicialización. Vincula el componente AudioSource y carga los efectos 
    /// de sonido desde la carpeta 'Resources'. Emite advertencias en consola si falta algo.
    /// Añade al texto de Ronda el numero de la ronda
    /// </summary>
    public void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        Time.timeScale = 1f;
        soundLostGame = Resources.Load<AudioClip>("soundLostGame");
        soundTakeLife = Resources.Load<AudioClip>("soundTakeLife");
        soundPause = Resources.Load<AudioClip>("soundPause");
        soundRestart = Resources.Load<AudioClip>("soundRestart");
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
        if(!soundPause)
        {
            Debug.LogWarning("No se ha encontrado soundPause");
        }
        if (!soundRestart)
        {
            Debug.LogWarning("No se ha encontrado soundRestart");
        }
        if(messageRound != null)
            messageRound.text = "Ronda " + countRound;
    }
    /// <summary>
    /// Se ejecuta antes que el Start. Ideal para limpiar variables estáticas 
    /// </summary>
    private void Awake()
    {
        countRound = 0;
        countMoney = 200;
        globalMoneyMultiplier = 1;
        globalCostMultiplier = 1;
        globalAttackSpeedMultiplier = 1;
        globalDamageTakenMultiplier = 1f;
        globalSpeedMultiplier = 1f;
        globalEnemyDamageMultiplier = 1f;
        globalEnemyHealthMultiplier = 1f;
    }
    /// <summary>
    /// Comprueba en cada frame si el Spawner indica que la ronda ha finalizado.
    /// En caso de que la ronda sea par y no sea la ronda 0, activa un evento random
    /// En caso afirmativo, prepara la siguiente oleada y actualiza el texto en pantalla.
    /// </summary>
    public void Update()
    {
        if (spawner != null && spawner.statusRound())
        {
            waitTime(5f);
            spawner.restartCountEnemy();
            messageRound.text = "Ronda " + countRound;

            if (countRound % roundsForCards == 0 && countRound != 0)
            {
                if(cardManager != null) cardManager.ShowCards();
            }
            
            if (countRound % 2 == 0 && countRound != 0)
            {
                Debug.Log(randomEvents.eventList.Count);
                if (randomEvents.eventList == null || randomEvents.eventList.Count == 0)
                {
                    this.GetComponent<randomEvents>().loadEventsInList();
                }
                int random = Random.Range(0, randomEvents.eventList.Count);
                StartCoroutine(randomEvents.eventList[random]());
                randomEvents.eventList.RemoveAt(random);
            
            }

            globalEnemyHealthMultiplier = Mathf.Pow(1.10f, countRound);
            globalEnemyDamageMultiplier = Mathf.Pow(1.05f, countRound);
        }
        timeinGame += Time.deltaTime;
        if(countMoneyText != null)
            countMoneyText.text = "Dinero: " + countMoney;
    }
    /// <summary>
    /// Corrutina de utilidad para pausar la ejecución de un proceso durante un tiempo determinado.
    /// </summary>
    /// <param name="time">El tiempo en segundos que el sistema debe esperar antes de continuar con la siguiente línea de código.</param>
    public IEnumerator waitTime(float time)
    {
        yield return new WaitForSeconds(time);
    }
    /// <summary>
    /// Gestiona la lógica de pausa del juego mostrando u ocultando el panel del menú.
    /// Alterna la escala de tiempo y reproduce un efecto de sonido dependiendo del estado actual.
    /// </summary>
    /// <param name="menuPanel">El GameObject que contiene la interfaz gráfica del menú de pausa.</param>
    public void pauseaandRestartButton(GameObject menuPanel)
    {
        changeTimeScale();
        bool status = menuPanel.activeSelf;
        menuPanel.SetActive(!status);
        AudioClip audioClip = Time.timeScale != 1.0f ? GameManager.soundPause : GameManager.soundRestart;
        sound(audioClip);
    }

    public void cambiarEstadoMusica()
    {
        musicaActiva = !musicaActiva;

        if (musicaActiva)
        {
            imagenBotonMusica.sprite = iconoMusicaOn;
            AudioListener.volume = 1f; // Activa el sonido global
        }
        else
        {
            imagenBotonMusica.sprite = iconoMusicaOff;
            AudioListener.volume = 0f; // Silencia el sonido global
        }
    }
    public void mainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("principalMenu");
    }
    /// <summary>
    /// Quita la pausa del juego y recarga la escena inicial para empezar una nueva partida.
    /// </summary>
    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
    public void cargarGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
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

    public static void sound(AudioClip audioClip)
    {
        if (audioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}