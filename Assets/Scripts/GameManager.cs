using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
// Las de VisualScripting y UnityEditor han sido eliminadas

/// <summary>
/// Controla la lógica general del nivel, la creación de defensas y el reinicio de la partida.
/// </summary>
public enum GameState { Playing, Paused, EventOpen }
public class GameManager : MonoBehaviour
{
    [Header("Ajustes de Sonido")]
    public GameObject botonMusica;
    public Image imagenBotonMusica;
    public Sprite iconoMusicaOn;
    public Sprite iconoMusicaOff;
    public static bool musicaActiva = true;

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
    // Efecto de sonido de Error.
    public static AudioClip soundError;
    // Efecto de sonido de Pagar.
    public static AudioClip soundPay;
    // Efecto de sonido de Ganar Dinero.
    public static AudioClip soundMoney;
    // Efecto de sonido para un Evento Especifico (EventCleanUpCosts).
    public static AudioClip soundEventCleanUpCosts;
    // Efecto de sonido Triste
    public static AudioClip soundSad;
      // Efecto de sonido Triste
    public static AudioClip soundBoss;
    // Efecto de sonido Para eventos donde salga ganando el Usuario (no es lo normal)
    public static AudioClip soundHappy;
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
    // Esta es la ÚNICA variable que controlará todo el juego
    public static GameState currentState = GameState.Playing;
    // Evita que el final de ronda se ejecute 60 veces seguidas 
    private bool isChangingRound = false;
    // Botón para continuar de ronda
    Button playButton;
    public Sprite playSprite;
    public static bool loadedFromSave = false;
    private bool waitingBetweenRounds = false;
    //velocidad actual del juego
    private int velocity = 0;

    [Header("Prefabs de Torres (para cargar partida)")]
    public GameObject prefabTowerMedian;    // mismo prefab que usa ConstructionMenu
    public GameObject prefabTowerLight;
    public GameObject prefabTowerHeavy;
    public GameObject prefabTowerInfernal;

    private int  _pendingCastleLife    = -1;
    private int  _pendingCastleLifeMax = -1;
    private bool _hasSavedGame         = false;
    private List<TowerSaveData> _pendingTowers = null;
    
    /// <summary>
    /// Método de inicialización. Vincula el componente AudioSource y carga los efectos 
    /// de sonido desde la carpeta 'Resources'. Emite advertencias en consola si falta algo.
    /// Añade al texto de Ronda el numero de la ronda
    /// </summary>
    public void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        Time.timeScale = 1f;
        Time.timeScale = 1f;
        if (AudioListener.volume == 0f)
        {
            musicaActiva = false;
            if (imagenBotonMusica != null) imagenBotonMusica.sprite = iconoMusicaOff;
        }
        else
        {
            musicaActiva = true;
            if (imagenBotonMusica != null) imagenBotonMusica.sprite = iconoMusicaOn;
        }
        if (GameObject.Find("PlayButton"))
        {
            playButton = GameObject.Find("PlayButton").GetComponent<Button>();

            if (loadedFromSave)
            {
                ShowPlayButton();
            }
            else
            {
                HidePlayButton();
            }
        }
        soundLostGame = Resources.Load<AudioClip>("soundLostGame");
        soundTakeLife = Resources.Load<AudioClip>("soundTakeLife");
        soundPause = Resources.Load<AudioClip>("soundPause");
        soundRestart = Resources.Load<AudioClip>("soundRestart");
        soundError = Resources.Load<AudioClip>("soundError");
        soundPay = Resources.Load<AudioClip>("soundPay");
        soundMoney = Resources.Load<AudioClip>("soundMoney");
        soundEventCleanUpCosts = Resources.Load<AudioClip>("soundEventCleanUpCosts");
        soundSad = Resources.Load<AudioClip>("soundSad");
        soundBoss= Resources.Load<AudioClip>("soundBoss");
        soundHappy = Resources.Load<AudioClip>("soundHappy");
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
        if (!soundError)
        {
            Debug.LogWarning("No se ha encontrado soundError");
        }
        if (!soundPay)
        {
            Debug.LogWarning("No se ha encontrado soundPay");
        }
        if (!soundMoney)
        {
            Debug.LogWarning("No se ha encontrado soundMoney");
        }
        if (!soundEventCleanUpCosts)
        {
            Debug.LogWarning("No se ha encontrado soundEventCleanUpCosts");
        }
        if (!soundSad)
        {
            Debug.LogWarning("No se ha encontrado soundSad");
        }
        if (!soundBoss)
        {
            Debug.LogWarning("No se ha encontrado soundBoss");
        }
        if (!soundHappy)
        {
            Debug.LogWarning("No se ha encontrado soundHappy");
        }
        if (messageRound != null)
            messageRound.text = "Ronda " + countRound;
        if (_hasSavedGame)
        {
            // Restaurar vida del castillo
            if (castlescript != null && _pendingCastleLife >= 0)
            {
                castlescript.life    = _pendingCastleLife;
                castlescript.lifeMax = _pendingCastleLifeMax;
            }
    
            // Restaurar torres
            if (_pendingTowers != null)
                RestoreTowers(_pendingTowers);
        }
    
        if (messageRound != null)
            messageRound.text = "Ronda " + countRound;
    }
    /// <summary>
    /// Se ejecuta antes que el Start. Ideal para limpiar variables estáticas 
    /// </summary>
    private void Awake()
    {
        GameSaveData saved = SaveSystem.Load();
 
        if (saved != null)
        {
            countRound                  = saved.countRound;
            countMoney                  = saved.countMoney;
            timeinGame                  = saved.timeinGame;
            enemiesDestroyed            = saved.enemiesDestroyed;
            countTower                  = saved.countTower;
            globalMoneyMultiplier       = saved.globalMoneyMultiplier;
            globalCostMultiplier        = saved.globalCostMultiplier;
            globalDamageTakenMultiplier = saved.globalDamageTakenMultiplier;
            globalAttackSpeedMultiplier = saved.globalAttackSpeedMultiplier;
            globalSpeedMultiplier       = saved.globalSpeedMultiplier;
            globalRadiusMultiplier      = saved.globalRadiusMultiplier;
            globalEnemyHealthMultiplier = saved.globalEnemyHealthMultiplier;
            globalEnemyDamageMultiplier = saved.globalEnemyDamageMultiplier;
 
            _pendingCastleLife    = saved.castleLife;
            _pendingCastleLifeMax = saved.castleLifeMax;
            _pendingTowers        = saved.towers;
            _hasSavedGame         = true;
            loadedFromSave = true;
            waitingBetweenRounds = true;
            GridGenerator.selectedGridIndex = saved.gridIndex;
        }
        else
        {
            countRound                  = 0;
            countMoney                  = 200;
            timeinGame = 0f;
            enemiesDestroyed = 0;
            countTower = 0;
            globalMoneyMultiplier       = 1;
            globalCostMultiplier        = 1f;
            globalAttackSpeedMultiplier = 1f;
            globalDamageTakenMultiplier = 1f;
            globalSpeedMultiplier       = 1f;
            globalEnemyDamageMultiplier = 1f;
            globalEnemyHealthMultiplier = 1f;
            loadedFromSave = false;
            waitingBetweenRounds = false;
            GridGenerator.selectedGridIndex = -1;
        }
    }
    /// <summary>
    /// Comprueba en cada frame si el Spawner indica que la ronda ha finalizado.
    /// En caso de que la ronda sea par y no sea la ronda 0, activa un evento random
    /// En caso afirmativo, prepara la siguiente oleada y actualiza el texto en pantalla.
    /// </summary>
    public void Update()
    {
        // Si estamos en pausa o cartas, no hacemos NADA
        if (GameManager.currentState != GameState.Playing) return;
        // Si ya estamos gestionando el cambio de ronda, esperamos
        if (isChangingRound) return;
        // Si la ronda acaba de terminar, lanzamos la linea de tiempo
        if (spawner != null && spawner.statusRound() && !waitingBetweenRounds)
        {
            StartCoroutine(endOfRoundRoutine());
        }
        // El tiempo y el dinero siguen su curso normal
        timeinGame += Time.deltaTime;
        if(countMoneyText != null)
            countMoneyText.text = "Dinero: " + countMoney;
        // Si se presiona la 'N' cambia la velocidad del juego
        if (Input.GetKeyDown(KeyCode.N))
        {
            changeVelocity();
        }
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
        if(GameManager.currentState == GameState.EventOpen) return;
        GameManager.currentState = GameManager.currentState == GameState.Paused ? GameState.Playing : GameState.Paused;
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

    private void ShowPlayButton()
    {
        if (playButton == null) return;

        playButton.interactable = true;

        Image spriteRenderer = playButton.GetComponent<Image>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = playSprite;
            spriteRenderer.color = Color.white;
        }
    }

    private void HidePlayButton()
    {
        if (playButton == null) return;

        playButton.interactable = false;

        Image spriteRenderer = playButton.GetComponent<Image>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color32(255, 255, 255, 0);
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
        SaveSystem.DeleteSave();
        ResetAllStaticVariables();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
    public void cargarGameOver()
    {
        SaveSystem.DeleteSave();
        Debug.Log(GameManager.timeinGame);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
    /// <summary>
    /// Cambia el time.TimeScale
    /// Si es 0 a 1
    /// Si no es 0 a 0
    /// </summary>
    public void changeTimeScale()
    {
        Time.timeScale = Time.timeScale == 0.0f ? (velocity+1.0f) : 0.0f;
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
   public IEnumerator endOfRoundRoutine()
   {
        isChangingRound = true;

        yield return new WaitForSeconds(2f);

        int nextRound = countRound + 1;

        // Cartas ANTES de parar entre rondas
        if (roundsForCards > 0 && nextRound % roundsForCards == 0)
        {
            if (cardManager != null)
            {
                currentState = GameState.EventOpen;
                cardManager.ShowCards();

                yield return new WaitUntil(() => currentState == GameState.Playing);
            }
        }

        waitingBetweenRounds = true;

        ShowPlayButton();

        SaveGame();

        isChangingRound = false;
   }

    public void playRound()
    {
        if (isChangingRound) return;
        if (!waitingBetweenRounds) return;

        StartCoroutine(StartNextRoundRoutine());
    }

    public void changeVelocity()
    {
        // Si está en pausa o con cartas, no se hace NADA
        if (GameManager.currentState != GameState.Playing) return;
        // Se actualiza la velocidad
        velocity = (velocity + 1) % 3;
        switch (velocity)
        {
            case 0:
                Time.timeScale = 1.0f;
                break;
            case 1:
                Time.timeScale = 2.0f;
                break;
            case 2:
                Time.timeScale = 3.0f;
                break;
            default:
                Time.timeScale = 1.0f;
                break;
        }
    }

    private IEnumerator StartNextRoundRoutine()
    {
        isChangingRound = true;

        HidePlayButton();

        waitingBetweenRounds = false;

        countRound++;

        globalEnemyHealthMultiplier = Mathf.Pow(1.10f, countRound);
        globalEnemyDamageMultiplier = Mathf.Pow(1.05f, countRound);

        if (messageRound != null)
            messageRound.text = "Ronda " + countRound;

        // Evento especial boss en ronda 10
        if (countRound == 10)
        {
            yield return new WaitForSeconds(1f);

            randomEvents eventsScript = GetComponent<randomEvents>();
            if (eventsScript != null)
            {
                StartCoroutine(eventsScript.EventBossRound());
           
            }
        }
        // Eventos normales
        else if (countRound % 2 == 0 && countRound != 0)
        {
            yield return new WaitForSeconds(1f);

            if (randomEvents.eventList == null || randomEvents.eventList.Count == 0)
            {
                GetComponent<randomEvents>().LoadEvents();
            }

            int random = Random.Range(0, randomEvents.eventList.Count);
            StartCoroutine(randomEvents.eventList[random]());
            randomEvents.eventList.RemoveAt(random);
        }

        if (spawner != null)
            spawner.restartCountEnemy();

        isChangingRound = false;
    }

    public void SaveGame()
    {
        var data = new GameSaveData
        {
            countRound                  = countRound,
            countMoney                  = countMoney,
            timeinGame                  = timeinGame,
            enemiesDestroyed            = enemiesDestroyed,
            countTower                  = countTower,
            castleLife                  = castlescript != null ? castlescript.life    : 100,
            castleLifeMax               = castlescript != null ? castlescript.lifeMax : 100,
            globalMoneyMultiplier       = globalMoneyMultiplier,
            globalCostMultiplier        = globalCostMultiplier,
            globalDamageTakenMultiplier = globalDamageTakenMultiplier,
            globalAttackSpeedMultiplier = globalAttackSpeedMultiplier,
            globalSpeedMultiplier       = globalSpeedMultiplier,
            globalRadiusMultiplier      = globalRadiusMultiplier,
            globalEnemyHealthMultiplier = globalEnemyHealthMultiplier,
            globalEnemyDamageMultiplier = globalEnemyDamageMultiplier,
            gridIndex = GridGenerator.selectedGridIndex,
        };
    
        Tower[] torres = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower t in torres)
        {
            if (!t.isBuilt) continue;
    
            // CORRECCIÓN: Buscamos correctamente en los hijos
            UpdateTower upTower = t.GetComponentInChildren<UpdateTower>(true);
            
            data.towers.Add(new TowerSaveData
            {
                posX              = t.transform.position.x,
                posY              = t.transform.position.y,
                towerType         = upTower != null ? upTower.typeOfTower : 0,
                level             = upTower != null ? upTower.levelOfTower : 0,
                totalGoldInvested = t.totalGoldInvested,
            });
        }
    
        SaveSystem.Save(data);
    }
    private void RestoreTowers(List<TowerSaveData> savedTowers)
    {
        // Iniciamos la rutina paso a paso
        StartCoroutine(RestoreTowersCoroutine(savedTowers));
    }

    private System.Collections.IEnumerator RestoreTowersCoroutine(List<TowerSaveData> savedTowers)
    {
        ConstructionMenu menu = FindAnyObjectByType<ConstructionMenu>();
        
        // Guardamos el dinero real y damos infinito temporalmente
        int realMoney = GameManager.countMoney;
        GameManager.countMoney = 9999999;
        GameManager.countTower = 0;
        foreach (TowerSaveData td in savedTowers)
        {
            GameObject prefab = td.towerType switch
            {
                0 => prefabTowerMedian,
                1 => prefabTowerLight,
                2 => prefabTowerHeavy,
                3 => prefabTowerInfernal,
                _ => null
            };
            if (prefab == null) continue;
 
            // 1. Asignamos el tipo para que esta torre concreta lo lea al nacer
            menu.flagTypeTower = td.towerType;

            // 2. Instanciamos la torre
            Vector3 pos = new Vector3(td.posX, td.posY, 0f);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            Tower tower = go.GetComponent<Tower>();
            
            if (tower != null)
            {
                // 3. ¡LA MAGIA! Esperamos 1 frame entero.
                // Esto permite que ESTA torre ejecute tranquilamente su Start() y se autoconstruya
                // antes de que el bucle avance e instancie la siguiente.
                yield return null; 
                
                // 4. Tras haberse construido sola, le aplicamos los niveles guardados
                UpdateTower updateTower = tower.GetComponentInChildren<UpdateTower>(true);
                if (updateTower != null)
                {
                    updateTower.typeOfTower = td.towerType;
                    for (int i = 0; i < td.level; i++)
                    {
                        int nextLevel = updateTower.levelOfTower + 1;
                        Sprite sprite = tower.towerImagen[nextLevel].GetComponent<SpriteRenderer>().sprite;
                        BoxCollider2D col = tower.towerImagen[nextLevel].GetComponent<BoxCollider2D>();
                
                        updateTower.levelOfTower++;
                        tower.updateFireCooldownAndDamage();
                        tower.setCollisionsAndSprite(tower.GetComponent<SpriteRenderer>(), sprite, col);
                    }
                }
                
                // 5. Restauramos su inversión de oro original
                tower.totalGoldInvested = td.totalGoldInvested;
            }
        }

        // 6. Al terminar de cargar todas las torres con éxito, devolvemos el dinero real
        GameManager.countMoney = realMoney;
    }
    /// <summary>
    /// Limpia de memoria todos los datos estáticos de la partida anterior.
    /// Se debe llamar antes de cargar una partida nueva limpia.
    /// </summary>
    public static void ResetAllStaticVariables()
    {
        countRound = 0;
        timeinGame = 0f;
        enemiesDestroyed = 0;
        countTower = 0;
        countMoney = 200; // El oro inicial que quieres darle al jugador

        // Multiplicadores
        globalMoneyMultiplier = 1;
        globalCostMultiplier = 1f;
        globalDamageTakenMultiplier = 1f;
        globalAttackSpeedMultiplier = 1f;
        globalSpeedMultiplier = 1f;
        globalRadiusMultiplier = 1f;
        globalEnemyHealthMultiplier = 1f;
        globalEnemyDamageMultiplier = 1f;

        // Estado del juego
        currentState = GameState.Playing;
        loadedFromSave = false;
    }
}