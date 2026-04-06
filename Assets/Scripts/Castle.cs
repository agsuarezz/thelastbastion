using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    // Actúa como un cerrojo de seguridad para garantizar que la lógica de Game Over y su sonido se ejecuten una única vez.
    private bool isGameOver = false;
    /// <summary>
    /// Método de inicialización. Establece la vida máxima basada en la vida inicial, 
    /// registra el valor en consola y se asegura de que el panel de Game Over empiece oculto.
    /// </summary>
    private void Start()
    {
        lifeMax = life;
        EndPanel.SetActive(false);
    }
    /// <summary>
    /// Se ejecuta en cada frame de las físicas del juego. 
    /// Actualiza continuamente el texto de la interfaz para mostrar la vida en formato "Actual/Máxima".
    /// </summary>
    private void FixedUpdate()
    {
        lifeText.text = life.ToString() + "/" + lifeMax;
    }
    /// <summary>
    /// Comprueba si el tag del objecto con el que colisiona, es enemigo 
    /// En caso de que si, lo destruye y le resta 20 vida al castillo
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy") && !isGameOver)
        {
           other.gameObject.GetComponent<Enemy>().DestroyEnemy();
            life -= 20;
            StartCoroutine(sound(GameManager.soundTakeLife));
            checkLife();
        }
    }
    /// <summary>
    /// Comprueba si el castillo se ha quedado sin life. 
    /// Si la life baja de 0, pausa el tiempo, suena musica de derrota y muestra el EndPanel de derrota.
    /// </summary>
    void checkLife()
    {
        if (life <= 0)
        {
            isGameOver = true;
            life = 0;
            GameManager.changeTimeScale();
            StartCoroutine(sound(GameManager.soundLostGame));
            EndPanel.SetActive(true);
            TextMeshProUGUI[] lista = EndPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach(TextMeshProUGUI text in lista)
            {
                if(text.name == "durationText")
                {
                    text.text = "Duracion: " + GameManager.timeinGame.ConvertTo<int>().ToString() + " s";
                }
                if(text.name == "enemiesDestroyedText")
                {
                    text.text = "Enemigo Derrotados: " + GameManager.enemiesDestroyed;
                }
                if (text.name == "countTowerText")
                {
                    text.text = "Torres Colocadas: " + GameManager.countTower;
                }
            }
        }
    }
    /// <summary>
    /// Corrutina que reproduce un clip de audio específico, espera exactamente el tiempo que dura dicho clip, 
    /// y luego limpia el reproductor para evitar solapamientos.
    /// </summary>
    /// <param name="audioCLip">El archivo de sonido que se va a reproducir.</param>
    public IEnumerator sound(AudioClip audioCLip)
    {
        GameManager.audioSource.clip = audioCLip;
        GameManager.audioSource.Play();
        yield return new WaitForSeconds(audioCLip.length);
        GameManager.audioSource.clip = null;
    }
}
