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

    [Header("Transición Game Over")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

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

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
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

    private void TriggerGameOver()
    {
        SaveSystem.DeleteSave();
        StartCoroutine(GameOverTransition());
    }

    private IEnumerator GameOverTransition()
    {
        GameManager.currentState = GameState.Paused;

        GameManager.sound(GameManager.soundLostGame);

        Time.timeScale = 0.3f;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene("GameOverPrueba");
    }

    public void SaveData()
    {
        int xpThisGame = calculateXP();

        GameManager.xpEarnedThisGame = xpThisGame;

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