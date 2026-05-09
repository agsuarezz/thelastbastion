using System.Collections;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

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
        {
            EndPanel.SetActive(false);
        }
        if(LogicaGameOver != null)
        {
            LogicaGameOver.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (lifeText != null)
        {
            lifeText.text = life + "/" + lifeMax;
        }

        if (lifeSlider != null)
        {
            lifeSlider.value = life;
        }
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


            FindObjectOfType<GameManager>().cargarGameOver();
        }
    }
    public void SaveData()
    {
        int xpThisGame = calculateXP();
        // 1. Cargamos lo que ya tenía el jugador en su cuenta persistente
        MetaSaveData metaPersistent = SaveSystem.LoadMeta();

        // 2. SUMAMOS la experiencia nueva a la que ya tenía
        metaPersistent.totalExperience += xpThisGame;
        SaveSystem.SaveMeta(metaPersistent);
        SaveSystem.DebugLogMetaSave();
    }
    public int calculateXP()
    {
        int baseRound = 10 * GameManager.countRound;
        int difficultyMultiplier = numberRoundHighTen() * 5;
        int bonusBoss = GameManager.hasBossAppeared ? 100: 0;
        int enemyDied = GameManager.enemiesDestroyed / 10;
        return baseRound + difficultyMultiplier + bonusBoss + enemyDied;
    }
    private int numberRoundHighTen()
    {
        return Mathf.Max(0, GameManager.countRound - 10);
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