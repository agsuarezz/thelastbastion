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
        loadEventsInList();
    }

    /// <summary>
    /// Evento Positivo: "Frenesí Capitalista".
    /// Duplica la recompensa de oro obtenida por cada enemigo derrotado durante 10 segundos.
    /// </summary>
    public IEnumerator EventLuckyGold()
    {
        GameManager.globalMoneyMultiplier *= 2;
        messageEvent.text = "Los duendes acaban de cobrar la nómina y traen los bolsillos llenos. ¡A por ellos!";
        GameManager.sound(GameManager.soundHappy);
        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= 2;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Positivo: "Rebajas del Black Friday".
    /// Torres a mitad de precio. Además, te devuelve dinero por las torres que ya tengas.
    /// </summary>
    public IEnumerator EventTowerDiscount()
    {
        GameManager.globalCostMultiplier *= 0.5f;

        // Buscamos cuántas torres hay ya construidas
        GameObject[] torresConstruidas = GameObject.FindGameObjectsWithTag("tower");

        messageEvent.text = "¡Black Friday en la herrería! Torres a mitad de precio.";
        GameManager.sound(GameManager.soundHappy);
        yield return new WaitForSeconds(10f);

        GameManager.globalCostMultiplier /= 0.5f;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Caótico: "Huelga de Arqueros".
    /// Las torres tardan el doble en disparar durante 10 segundos.
    /// Requiere aplicar la variable en el temporizador de disparo del script de la Torre.
    /// </summary>
    public IEnumerator EventArcherStrike()
    {
        GameManager.globalAttackSpeedMultiplier *= 4f;
        messageEvent.text = "Sindicato de tiradores en huelga. Disparos más lentos.";
        GameManager.sound(GameManager.soundSad);
        yield return new WaitForSeconds(7f);
        GameManager.globalAttackSpeedMultiplier /= 4f;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Negativo: "El Cobrador".
    /// Hacienda se lleva el 40% de tus ahorros actuales. ¡Duele más cuanto más rico eres!
    /// </summary>
    public IEnumerator EventTaxCollector()
    {
        int taxes = (int)(GameManager.countMoney * 0.40f); // Calcula el 40%
        GameManager.countMoney -= taxes;
        messageEvent.text = $"El inspector de Hacienda te ha confiscado {taxes} de Oro por no declarar las torres.";
        GameManager.sound(GameManager.soundPay);
        yield return new WaitForSeconds(5f);
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Negativo (Cruel): "Tasa de Limpieza".
    /// El multiplicador de dinero pasa a negativo. ¡Pagas por cada baja!
    /// </summary>
    public IEnumerator EventCleanUpCosts()
    {
        GameManager.globalMoneyMultiplier *= -2;
        messageEvent.text = "Tasa ecológica activa. Ahora PAGAS tú por limpiar los cadáveres de duende.";
        GameManager.sound(GameManager.soundEventCleanUpCosts);
        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= -2;
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Caótico: "Subidón de Azúcar".
    /// Los enemigos corren casi al triple de velocidad, pero reciben el DOBLE de daño.
    /// </summary>
    public IEnumerator EventSugarRush()
    {

        GameManager.globalSpeedMultiplier *= 2.5f;
        GameManager.globalDamageTakenMultiplier *= 2f;
        messageEvent.text = "¡Alguien les dio bebida energética! Corren como locos pero son de cristal.";
        GameManager.sound(GameManager.soundSad);
        yield return new WaitForSeconds(10f);

        GameManager.globalSpeedMultiplier /= 2.5f;
        GameManager.globalDamageTakenMultiplier /= 2f;
        messageEvent.text = "";
    }

    /// <summary>
    /// Evento Positivo: "Lluvia de Inversiones".
    /// Hace llover 5 monedas del cielo en posiciones aleatorias para que el jugador las recoja.
    /// </summary>
    public IEnumerator EventCoinRain()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-10f, 10f), 10f, 0f);
            yield return new WaitForSeconds(0.1f);
            Instantiate(Resources.Load<GameObject>("prefabCoins"), spawnPos, Quaternion.identity);
        }
        messageEvent.text = "Bug detectado: Lluvia de monedas. ¡Aprovecha antes del parche!";
        yield return new WaitForSeconds(5f);
        messageEvent.text = "";
    }
    /// <summary>
    /// Evento Caótico: "Spam de Anuncios".
    /// Carga y genera anuncios aleatorios en la interfaz para distraer al jugador.
    /// </summary>
    public IEnumerator EventSpawnAds()
    {
        // Mensaje irónico en pantalla
        messageEvent.text = "Somos un estudio Indie, danos tu dinero";

        // 1. CARGA OPTIMIZADA: Cargamos los 4 prefabs en un array (arreglo)
        // Lo hacemos una sola vez aquí fuera para no saturar el procesador
        GameObject[] adPrefabs = new GameObject[4];
        adPrefabs[0] = Resources.Load<GameObject>("prefabNew");
        adPrefabs[1] = Resources.Load<GameObject>("prefabNew1");
        adPrefabs[2] = Resources.Load<GameObject>("prefabNew2");
        adPrefabs[3] = Resources.Load<GameObject>("prefabNew3");

        // 2. CACHEO: Buscamos el objeto padre en el Canvas una sola vez
        Transform parentCanvas = GameObject.Find("Canvas_General").transform;

        // 3. BUCLE DE GENERACIÓN: Vamos a crear 3 anuncios
        for (int i = 0; i < 4; i++)
        {
            // Elegimos un prefab al azar de nuestro array
            GameObject selectedPrefab = adPrefabs[UnityEngine.Random.Range(0, adPrefabs.Length)];

            // Instanciamos el anuncio. El 'false' evita que Unity recalcule escalas 3D innecesarias
            GameObject spawnedAd = Instantiate(selectedPrefab, parentCanvas, false);

            // Obtenemos el RectTransform para manipular la posición en la UI
            RectTransform adRect = spawnedAd.GetComponent<RectTransform>();

            // Calculamos coordenadas aleatorias dentro de un rango seguro para 1080p
            float randomX = UnityEngine.Random.Range(-700f, 700f);
            float randomY = UnityEngine.Random.Range(-200f, 200f);

            // Aplicamos la posición final
            adRect.anchoredPosition = new Vector2(randomX, randomY);

            // Pequeña espera entre cada spawn para que no salgan todos de golpe
            yield return new WaitForSeconds(0.15f);
        }

        // Mantenemos el mensaje 5 segundos antes de borrarlo
        yield return new WaitForSeconds(5f);
        messageEvent.text = "";
    }
    public void loadEventsInList()
    {
        // Cargamos el catálogo COMPLETO de eventos disponibles en el juego
        eventList.Add(EventSpawnAds);        // Positivo: Lluvia de monedas (minijuego)
                                              // Cargamos el catálogo COMPLETO de eventos disponibles en el juego
        //eventList.Add(EventLuckyGold);       // Positivo: Doble de oro
        //eventList.Add(EventTowerDiscount);   // Positivo: Torres a mitad de precio
        //eventList.Add(EventArcherStrike);    // Negativo: Torres disparan lento
        //eventList.Add(EventTaxCollector);    // Negativo: Hacienda te roba el 20%
        //eventList.Add(EventCleanUpCosts);    // Negativo/Cruel: Pagas por matar
        //eventList.Add(EventSugarRush);       // Caótico: Enemigos rápidos pero frágiles
        //eventList.Add(EventCoinRain);        // Positivo: Lluvia de monedas (minijuego)
    }
}