using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverStatsController : MonoBehaviour
{
    [SerializeField] private string gameOverSceneName = "GameOverPrueba";

    public void GoBackToGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }
}