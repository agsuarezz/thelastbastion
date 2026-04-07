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
        eventList.Add(EventTowerDiscount);
        eventList.Add(EventTankEnemies);
        eventList.Add(EventArcherStrike);
    }

    /// <summary>
    /// Evento Negativo: "Estampida de Duendes". 
    /// Duplica la velocidad de movimiento de todos los enemigos en el mapa durante 5 segundos.
    /// </summary>
    public IEnumerator EventGoblinStampede()
    {
        GameManager.globalSpeedMultiplier = 2f;
        messageEvent.text = "Error 404: Exceso de cafeína detectado en el enemigo.";

        yield return new WaitForSeconds(5f);

        GameManager.globalSpeedMultiplier = 1f;
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
    /// <summary>
    /// Evento Positivo: "Rebajas del Black Friday".
    /// Reduce a la mitad el coste de construcción de todas las torres durante 10 segundos.
    /// Requiere aplicar 'globalCostMultiplier' al precio de la torre antes de restar el oro.
    /// </summary>
    public IEnumerator EventTowerDiscount()
    {
        GameManager.globalCostMultiplier = 0.5f;
        messageEvent.text = "¡Black Friday en la herrería! Torres a mitad de precio";
        yield return new WaitForSeconds(10f);
        GameManager.globalCostMultiplier = 1f;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Negativo: "Armaduras de Amazon Prime".
    /// Los enemigos reciben la mitad de daño durante 10 segundos.
    /// Requiere aplicar 'Enemy.globalDamageTakenMultiplier' en el método TakeDamage del enemigo.
    /// </summary>
    public IEnumerator EventTankEnemies()
    {
        GameManager.globalDamageTakenMultiplier = 0.5f;
        messageEvent.text = "Los duendes se han puesto doble calzoncillo. Reciben menos daño.";
        yield return new WaitForSeconds(10f);
        GameManager.globalDamageTakenMultiplier = 1f;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Caótico: "Huelga de Arqueros".
    /// Las torres tardan el doble en disparar durante 10 segundos.
    /// Requiere aplicar la variable en el temporizador de disparo del script de la Torre.
    /// </summary>
    public IEnumerator EventArcherStrike()
    {
        GameManager.globalAttackSpeedMultiplier = 2;
        messageEvent.text = "Sindicato de tiradores en huelga. Disparos más lentos.";
        yield return new WaitForSeconds(10f);
        GameManager.globalAttackSpeedMultiplier = 1;
        messageEvent.text = "";
    }
}