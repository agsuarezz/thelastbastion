using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestor principal de los eventos aleatorios del juego.
/// Mantiene el catálogo de eventos (positivos y negativos) que pueden alterar temporalmente las reglas de la partida.
/// </summary>
public class randomEvents : MonoBehaviour
{
    [Header("Configuración de Eventos")]

    /// <summary>
    /// Lista global que almacena las referencias a las funciones de los eventos disponibles.
    /// Permite disparar eventos al azar eligiendo un índice aleatorio de esta colección.
    /// </summary>
    public static List<System.Func<IEnumerator>> eventList;

    /// <summary>
    /// Referencia al componente de texto en la interfaz gráfica (UI) donde se mostrarán los mensajes irónicos.
    /// </summary>
    public TextMeshProUGUI messageEvent;

    private void Start()
    {
        // Inicializamos la lista vacía para evitar errores de referencia nula
        eventList = new List<System.Func<IEnumerator>>();

        // Cargamos el catálogo de eventos disponibles en el juego
        eventList.Add(EventGoblinStampede);
        eventList.Add(EventLuckyGold);
    }

    /// <summary>
    /// Evento Negativo: "Estampida de Duendes". 
    /// Duplica la velocidad de movimiento de todos los enemigos en el mapa durante 5 segundos.
    /// </summary>
    public IEnumerator EventGoblinStampede()
    {
        Enemy.globalSpeedMultiplier = 2f;
        messageEvent.text = "Error 404: Exceso de cafeína detectado en el enemigo.";

        yield return new WaitForSeconds(5f);

        Enemy.globalSpeedMultiplier = 1f;
        messageEvent.text = "";
    }

    /// <summary>
    /// Evento Positivo: "Frenesí Capitalista".
    /// Duplica la recompensa de oro obtenida por cada enemigo derrotado durante 10 segundos.
    /// </summary>
    public IEnumerator EventLuckyGold()
    {
        GameManager.globalMoneyMultiplier = 2;
        messageEvent.text = "Hacienda te ha devuelto la declaración. ¡Disfruta el bonus!";

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier = 1;
        messageEvent.text = "";
    }
}