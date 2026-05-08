using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// Gestor principal encargado de administrar y ejecutar los eventos dinámicos del juego.
/// Controla la aparición de eventos positivos y negativos utilizando un sistema de ruleta
/// con pesos dinámicos que escala la dificultad de forma infinita según la ronda actual.
/// También gestiona el feedback visual y sonoro en la interfaz de usuario.
/// </summary>

public class EventManager : MonoBehaviour
{
    [Header("Catálogo de Eventos")]
    public List<DynamicEvent> allEvents;
    [Tooltip("Componente de texto UI para mostrar los mensajes de los eventos.")]
    public TextMeshProUGUI messageEventText;

    [Tooltip("Panel/Imagen de fondo para los mensajes de los eventos.")]
    public GameObject eventBackgroundUI;
    [Tooltip("Duración del texto y fondo del evento en pantalla.")]
    public float eventUIDuration = 5f;

    private Coroutine hideUICoroutine;
    /// <summary>
    /// Método de inicialización de Unity. Se asegura de que el panel de la interfaz 
    /// de los eventos esté completamente oculto al comenzar la partida.
    /// </summary>
    void Start()
    {
        HideEventUI();
    }
    /// <summary>
    /// Inicia la secuencia de un nuevo evento dinámico. Filtra si es una ronda de jefe 
    /// (cada 10 rondas) para forzar su aparición; en rondas normales, delega la selección 
    /// al sistema de ruleta por pesos. Despliega la UI y ejecuta la lógica del evento.
    /// </summary>
    /// <param name="currentWave">El número de la oleada actual para calcular la dificultad.</param>
    public void TriggerEvent(int currentWave)
    {
        if (allEvents == null || allEvents.Count == 0)
        {
            Debug.LogWarning("¡Jefe, no has metido los eventos en la lista del Inspector!");
            return;
        }
        DynamicEvent selectedEvent;
        if (currentWave % 10 == 0)
        {
            // Forzamos el evento del Boss
            selectedEvent = allEvents.Find(e => e is EventBossRound);
        }
        else
        {
            // Si es una ronda normal, tiramos la ruleta de pesos
            selectedEvent = SelectEventByWeight(currentWave);
        }

        // 1. Mostramos el mensaje sarcástico en pantalla y reproducimos el sonido
        ShowEventUI(selectedEvent.description, selectedEvent.audioClip);

        // 2. Ejecutamos la lógica del evento (Como somos MonoBehaviour, podemos usar Corrutinas)
        StartCoroutine(selectedEvent.Execute());
    }
    /// <summary>
    /// Calcula y selecciona un evento aleatorio utilizando un sistema de probabilidad ponderada. 
    /// Ignora eventos forzados (como el Boss) y aplica un escalado matemático de "crueldad" 
    /// para hacer que los eventos negativos sean más frecuentes en rondas avanzadas.
    /// </summary>
    /// <param name="currentWave">La oleada actual, utilizada como multiplicador de crueldad.</param>
    /// <returns>El evento dinámico (DynamicEvent) ganador de la ruleta.</returns>
    private DynamicEvent SelectEventByWeight(int currentWave)
    {
        float totalWeight = 0f;
        List<float> calculatedWeights = new List<float>();

        // Creamos una lista temporal solo con los eventos que SÍ pueden salir al azar
        List<DynamicEvent> validRandomEvents = new List<DynamicEvent>();

        foreach (var ev in allEvents)
        {
            // Si el evento es el del Boss, lo ignoramos y no entra en la ruleta
            if (ev is EventBossRound) continue;

            float currentWeight = ev.baseWeight;

            // LÓGICA DE ESCALADO INFINITO
            if (currentWave <= 3)
            {
                if (ev.type == EventType.Harmful) currentWeight *= 0.2f;
                if (ev.type == EventType.Beneficial) currentWeight *= 1.5f;
            }
            else if (currentWave <= 9)
            {
                // Fase Normal
            }
            else
            {
                // Aumento de la dificultad
                float crueltyLevel = (currentWave - 9) * 0.1f;

                if (ev.type == EventType.Harmful) currentWeight *= (1.5f + crueltyLevel);
                if (ev.type == EventType.Beneficial) currentWeight *= Mathf.Max(0.1f, 1f - crueltyLevel);
            }

            validRandomEvents.Add(ev);
            calculatedWeights.Add(currentWeight);
            totalWeight += currentWeight;
        }

        // Tirar el "dado" de la ruleta
        float randomValue = Random.Range(0f, totalWeight);

        // Ver qué evento ha ganado en nuestra lista de eventos válidos
        for (int i = 0; i < validRandomEvents.Count; i++)
        {
            randomValue -= calculatedWeights[i];
            if (randomValue <= 0f) return validRandomEvents[i];
        }

        return validRandomEvents[validRandomEvents.Count - 1];
    }
    /// <summary>
    /// Oculta instantáneamente el panel visual de eventos y limpia el texto en pantalla.
    /// </summary>
    private void HideEventUI()
    {
        if (messageEventText != null) messageEventText.text = "";
        if (eventBackgroundUI != null) eventBackgroundUI.SetActive(false);
    }
    /// <summary>
    /// Despliega el panel de evento en la interfaz, mostrando el mensaje descriptivo y 
    /// reproduciendo el clip de audio asociado. Si ya había un evento mostrándose, 
    /// reinicia el temporizador de ocultación.
    /// </summary>
    /// <param name="message">El texto (sarcástico o informativo) que leerá el jugador.</param>
    /// <param name="soundEffect">El efecto de sonido que acompañará al pop-up (puede ser nulo).</param>
    private void ShowEventUI(string message, AudioClip soundEffect)
    {
        if (messageEventText != null) messageEventText.text = message;
        if (eventBackgroundUI != null) eventBackgroundUI.SetActive(true);
        if (soundEffect != null) GameManager.sound(soundEffect);

        if (hideUICoroutine != null)
            StopCoroutine(hideUICoroutine);

        hideUICoroutine = StartCoroutine(HideEventUIAfterSeconds());
    }
    /// <summary>
    /// Corrutina encargada de mantener el panel del evento en pantalla durante los segundos 
    /// configurados en la variable eventUIDuration antes de ocultarlo automáticamente.
    /// </summary>
    private IEnumerator HideEventUIAfterSeconds()
    {
        yield return new WaitForSeconds(eventUIDuration);
        HideEventUI();
    }

}
