using TMPro;
using UnityEngine;

public class GameOverStatsUI : MonoBehaviour
{
    [Header("Textos de valores")]
    [SerializeField] private TextMeshProUGUI roundsValueText;
    [SerializeField] private TextMeshProUGUI enemiesValueText;
    [SerializeField] private TextMeshProUGUI goldValueText;
    [SerializeField] private TextMeshProUGUI towersValueText;
    [SerializeField] private TextMeshProUGUI timeValueText;

    private void Start()
    {
        roundsValueText.text = GameManager.countRound.ToString();
        enemiesValueText.text = GameManager.enemiesDestroyed.ToString();
        goldValueText.text = GameManager.totalGoldEarned.ToString();
        towersValueText.text = GameManager.countTower.ToString();
        timeValueText.text = FormatTime(GameManager.timeinGame);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}