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
    /// Gira la ruleta de eventos. Suma los pesos de todos los eventos válidos (aplicando 
    /// el castigo por ronda) y saca una papeleta al azar.
    /// </summary>
    private DynamicEvent SelectEventByWeight(int currentWave)
    {
        List<DynamicEvent> validEvents = new List<DynamicEvent>();
        List<float> finalWeights = new List<float>();
        float totalWeight = 0f;

        // 1. PREPARAMOS EL BOMBO DE LOTERÍA
        foreach (var ev in allEvents)
        {
            // El Boss no juega a esta ruleta, sale cuando le toca
            if (ev is EventBossRound) continue;

            // Preguntamos a nuestro otro método cuántos "boletos" merece este evento hoy
            float weightForThisRound = CalculateCrueltyWeight(ev, currentWave);

            validEvents.Add(ev);
            finalWeights.Add(weightForThisRound);
            totalWeight += weightForThisRound; // Agrandamos el bombo
        }

        // 2. TIRAMOS EL DADO (Mano inocente saca un número del 0 al total de boletos)
        float randomValue = Random.Range(0f, totalWeight);

        // 3. REVISAMOS QUIÉN HA GANADO
        for (int i = 0; i < validEvents.Count; i++)
        {
            randomValue -= finalWeights[i];

            // Cuando la resta llega a 0 o menos, hemos encontrado el boleto ganador
            if (randomValue <= 0f)
            {
                return validEvents[i];
            }
        }

        // Por seguridad, si Unity se lía con los decimales, devolvemos el último
        return validEvents[validEvents.Count - 1];
    }

    /// <summary>
    /// Aquí reside la maldad pura. Coge el peso base de un evento y lo muta 
    /// dependiendo de en qué ronda estemos. Cuanto más avanzas, más se dopan los eventos malos.
    /// </summary>
    private float CalculateCrueltyWeight(DynamicEvent ev, int currentWave)
    {
        float weight = ev.baseWeight; // Los boletos iniciales que tiene este evento

        // FASE 1: Tutorial (Rondas 1-3). Somos buenos con el jugador.
        if (currentWave <= 3)
        {
            if (ev.type == EventType.Harmful) return weight * 0.2f;    // Le quitamos el 80% de los boletos malos
            if (ev.type == EventType.Beneficial) return weight * 1.5f; // Le regalamos un 50% extra de boletos buenos
            return weight; // Los neutros se quedan igual
        }

        // FASE 2: Normal (Rondas 4-9). El juego es justo.
        if (currentWave <= 9)
        {
            return weight;
        }

        // FASE 3: El Infierno (Rondas 10+). Que empiece el sufrimiento.
        float crueltyLevel = (currentWave - 9) * 0.1f; // Sube 0.1 por cada ronda extra

        if (ev.type == EventType.Harmful)
        {
            // Un evento malo multiplica sus boletos salvajemente
            return weight * (1.5f + crueltyLevel);
        }

        if (ev.type == EventType.Beneficial)
        {
            // Un evento bueno pierde sus boletos, pero nunca baja de 0.1f para mantener la esperanza viva
            return weight * Mathf.Max(0.1f, 1f - crueltyLevel);
        }

        // Los neutrales siempre mantienen su peso base
        return weight;
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
