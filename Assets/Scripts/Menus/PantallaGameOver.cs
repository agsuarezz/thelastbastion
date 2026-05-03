using UnityEngine;
using TMPro;

public class PantallaGameOver : MonoBehaviour
{
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI towersText;

    void Start()
    {
        // Al empezar la escena de GameOver, leemos los datos estáticos del GameManager
        durationText.text = "Duracion: " + (int)GameManager.timeinGame + " s";
        enemiesText.text = "Enemigos Derrotados: " + GameManager.enemiesDestroyed;
        towersText.text = "Torres Construidas: " + GameManager.countTower;
        GameManager.sound(GameManager.soundLostGame);
    }

    public void VolverAlMenu()
    {
        GameManager.ResetAllStaticVariables();
        UnityEngine.SceneManagement.SceneManager.LoadScene("PrincipalMenu");
    }
}