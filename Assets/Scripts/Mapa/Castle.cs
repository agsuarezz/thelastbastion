using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la vida del castillo, actualiza la UI y dispara el Game Over.
/// </summary>
public class castleScript : MonoBehaviour
{
    [Tooltip("Texto y Slider de la UI donde se muestra la vida restante.")]
    public TextMeshProUGUI lifeText;
    public Slider lifeSlider;

    [Tooltip("Cantidad inicial de vida del jugador o la base.")]
    public int life = 100;

    public int lifeMax;

    [Tooltip("Panel de Game Over o Victoria que se mostrará al final.")]
    public GameObject EndPanel;

    [Tooltip("BoxCollider2D Castillo.")]
    public BoxCollider2D castleCollider;

    private bool isGameOver = false;
    public GameObject LogicaGameOver;

    private void Start()
    {
        StartCoroutine(SearchandMovetotheMap());
        lifeMax = life;

        if (lifeSlider != null)
        {
            lifeSlider.maxValue = lifeMax;
            lifeSlider.value = life;
        }

        if (EndPanel != null)
            EndPanel.SetActive(false);

        if (LogicaGameOver != null)
            LogicaGameOver.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (lifeText != null)
            lifeText.text = life + "/" + lifeMax;

        if (lifeSlider != null)
            lifeSlider.value = life;
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return;

        life -= damage;
        GameManager.sound(GameManager.soundTakeLife);
        if (life < 0)
            life = 0;
        CheckLife();
    }

    private void CheckLife()
    {
        if (life <= 0 && !isGameOver)
        {
            isGameOver = true;
            life = 0;

            SaveData();
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Congela el mapa tal como está y carga la escena GameOver encima (Additive),
    /// de modo que el fondo sigue siendo el mapa en el momento de la derrota.
    /// </summary>
    private void TriggerGameOver()
    {
        // 1. Borramos la partida guardada
        SaveSystem.DeleteSave();

        // 2. Congelamos el tiempo: enemigos, torres y animaciones quedan parados
        Time.timeScale = 0f;

        // 3. Actualizamos el estado global
        GameManager.currentState = GameState.Paused;

        // 4. Sonido de derrota
        GameManager.sound(GameManager.soundLostGame);

        // 5. Cargamos la escena GameOver ENCIMA de la escena actual (sin destruirla)
        //    Así el mapa sigue visible de fondo
        SceneManager.LoadScene("GameOverPrueba", LoadSceneMode.Additive);
    }

    public void SaveData()
    {
        int xpThisGame = calculateXP();
        MetaSaveData metaPersistent = SaveSystem.LoadMeta();
        metaPersistent.totalExperience += xpThisGame;
        SaveSystem.SaveMeta(metaPersistent);
        SaveSystem.DebugLogMetaSave();
    }

    public int calculateXP()
    {
        int baseRound = 10 * GameManager.countRound;
        int difficultyMultiplier = 10 * (GameManager.countRound / 100);
        int bonusBoss = 100 * (GameManager.countRound / 10);
        return baseRound + difficultyMultiplier + bonusBoss;
    }

    public IEnumerator SearchandMovetotheMap()
    {
        yield return new WaitForEndOfFrame();
        CastleSpawnPoint castleSpawnPoint = FindAnyObjectByType<CastleSpawnPoint>();
        if (castleSpawnPoint != null)
        {
            transform.position = castleSpawnPoint.CalculatedCenter;
        }
        else
        {
            Debug.LogWarning("El Castillo no encontró ningún MapConfig en la escena.");
        }
    }
}