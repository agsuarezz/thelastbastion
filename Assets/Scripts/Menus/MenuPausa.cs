using UnityEngine;
using UnityEngine.UI; // ¡Importante para manejar imágenes!

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausaUI;
    public GameObject botonMusica; // Arrastra aquí el botón de música
    public Image imagenBotonMusica; // Arrastra aquí el componente Image del botón música

    public Sprite iconoMusicaOn;  // Sprite de música normal
    public Sprite iconoMusicaOff; // Sprite de música tachada

    private bool musicaActiva = true;

    // 1. Función para mostrar/ocultar el botón de música
    public void ToggleVisibilidadMusica()
    {
        // Si está activo lo oculta, si está oculto lo muestra
        botonMusica.SetActive(!botonMusica.activeSelf);
    }

    // 2. Función para cambiar el estado de la música (On/Off)
    public void CambiarEstadoMusica()
    {
        musicaActiva = !musicaActiva;

        if (musicaActiva)
        {
            imagenBotonMusica.sprite = iconoMusicaOn;
            // Aquí en un futuro pondrás: MiFuenteDeAudio.Play();
            AudioListener.volume = 1f; // Esto activa todo el sonido de Unity
            Debug.Log("Música Activada");
        }
        else
        {
            imagenBotonMusica.sprite = iconoMusicaOff;
            // Aquí en un futuro pondrás: MiFuenteDeAudio.Pause();
            AudioListener.volume = 0f; // Esto silencia TODO el juego de golpe
            Debug.Log("Música Silenciada");
        }
    }

    // ... tus otras funciones de Pausar() y Reanudar()
}