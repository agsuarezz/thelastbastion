using TMPro;
using UnityEngine;
/// <summary>
/// Gestiona la life del castillo, actualiza la UI y dispara el Game Over.
/// </summary>
public class castleScript : MonoBehaviour
{
    // Referencia al texto en la interfaz que muestra la vida actual.
    [Tooltip("Texto de la UI donde se muestra la vida restante.")]
    public TextMeshProUGUI lifeText;
    // Puntos de vida actuales de la base o del jugador (Inicia en 100).
    [Tooltip("Cantidad inicial de vida del jugador o la base.")]
    public int life = 100;
    // Almacena la cantidad máxima de vida permitida para evitar sobrecuración.
    int lifeMax;
    // Referencia al panel que se activa cuando la partida termina.
    [Tooltip("Panel de Game Over o Victoria que se mostrará al final.")]
    public GameObject EndPanel;
    // Referencia al collider del Castillo
    [Tooltip("BoxCollider2d Castillo.")]
    public BoxCollider2D castleCollider;
    private void Start()
    {
        Debug.Log(life);
        lifeMax = life;
        EndPanel.SetActive(false);
    }
    private void FixedUpdate()
    {
        lifeText.text = life.ToString() + "/" + lifeMax;
        checkLife();
        if(castleCollider.CompareTag("Enemy"))
            Debug.Log(life);
    }
    /// <summary>
    /// Comprueba si el tag del objecto con el que colisiona, es enemigo 
    /// En caso de que si, lo destruye y le resta 20 vida al castillo
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
           other.gameObject.GetComponent<Enemy>().DestroyEnemy();
            life -= 20;
        }
    }
    /// <summary>
    /// Comprueba si el castillo se ha quedado sin life. 
    /// Si la life baja de 0, pausa el tiempo y muestra el EndPanel de derrota.
    /// </summary>
    void checkLife()
    {
        if (life <= 0)
        {
            Time.timeScale = 0f;
            EndPanel.SetActive(true);
        }
    }
}
