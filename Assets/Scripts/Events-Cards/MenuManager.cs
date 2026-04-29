using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void EmpezarJuego()
    {
        SceneManager.LoadScene("Main");
    }
}
