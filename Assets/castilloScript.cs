using TMPro;
using UnityEngine;
/// <summary>
/// Gestiona la vida del castillo, actualiza la UI y dispara el Game Over.
/// </summary>
public class castilloScript : MonoBehaviour
{
    public TextMeshProUGUI textoVida;
    public int vida = 100;
    int vidaMax;
    public GameObject panel;
    private void Start()
    {
        Debug.Log(vida);
        vidaMax = vida;
        panel.SetActive(false);
    }
    private void FixedUpdate()
    {
        textoVida.text = vida.ToString() + "/" + vidaMax;
        //vida -= 1;
        comprobarVida();
    }
    /// <summary>
    /// Comprueba si el castillo se ha quedado sin vida. 
    /// Si la vida baja de 0, pausa el tiempo y muestra el panel de derrota.
    /// </summary>
    void comprobarVida()
    {
        if(vida < 0)
        {
            Time.timeScale = 0f;
            panel.SetActive(true);
        }
    }
}
