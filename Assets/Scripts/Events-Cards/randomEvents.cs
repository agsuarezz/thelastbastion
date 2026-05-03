using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestor principal para los eventos aleatorios del juego.
/// Maneja el catálogo de eventos (positivos y negativos) que alteran temporalmente las reglas de juego.
/// </summary>
public class randomEvents : MonoBehaviour
{
    [Header("Configuración de Eventos")]
    public static List<System.Func<IEnumerator>> eventList;

    [Tooltip("Componente de texto UI para mostrar los mensajes de los eventos.")]
    public TextMeshProUGUI messageEventText;

    [Tooltip("Panel/Imagen de fondo para los mensajes de los eventos.")]
    public GameObject eventBackgroundUI;

    private void Start()
    {
        eventList = new List<System.Func<IEnumerator>>();
        LoadEvents();
        HideEventUI();
    }

 [Tooltip("Duración del texto y fondo del evento en pantalla.")]
public float eventUIDuration = 5f;

private Coroutine hideUICoroutine;

private void ShowEventUI(string message, AudioClip soundEffect)
{
    if (messageEventText != null) messageEventText.text = message;
    if (eventBackgroundUI != null) eventBackgroundUI.SetActive(true);
    if (soundEffect != null) GameManager.sound(soundEffect);

    if (hideUICoroutine != null)
        StopCoroutine(hideUICoroutine);

    hideUICoroutine = StartCoroutine(HideEventUIAfterSeconds());
}

private IEnumerator HideEventUIAfterSeconds()
{
    yield return new WaitForSeconds(eventUIDuration);
    HideEventUI();
}

    private void HideEventUI()
    {
        if (messageEventText != null) messageEventText.text = "";
        if (eventBackgroundUI != null) eventBackgroundUI.SetActive(false);
    }

    // --- EVENTOS ALEATORIOS ---

    public IEnumerator EventLuckyGold()
    {
        GameManager.globalMoneyMultiplier *= 2;
        ShowEventUI("Los goblins acaban de cobrar y tienen los bolsillos llenos. ¡A por ellos!", GameManager.soundHappy);

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= 2;
        HideEventUI();
    }

    public IEnumerator EventTowerDiscount()
    {
        GameManager.globalCostMultiplier *= 0.5f;
        ShowEventUI("¡Black Friday en la herrería! Las torres están a mitad de precio.", GameManager.soundHappy);

        yield return new WaitForSeconds(10f);

        GameManager.globalCostMultiplier /= 0.5f;
        HideEventUI();
    }

    public IEnumerator EventArcherStrike()
    {
        GameManager.globalAttackSpeedMultiplier *= 4f;
        ShowEventUI("Sindicato de arqueros en huelga. Velocidad de ataque reducida significativamente.", GameManager.soundSad);

        yield return new WaitForSeconds(7f);

        GameManager.globalAttackSpeedMultiplier /= 4f;
        HideEventUI();
    }

    public IEnumerator EventTaxCollector()
    {
        int taxes = (int)(GameManager.countMoney * 0.40f);
        GameManager.countMoney -= taxes;
        ShowEventUI($"El recaudador de impuestos confiscó {taxes} de oro por estructuras defensivas no declaradas.", GameManager.soundPay);

        yield return new WaitForSeconds(5f);

        HideEventUI();
    }

    public IEnumerator EventCleanUpCosts()
    {
        GameManager.globalMoneyMultiplier *= -2;
        ShowEventUI("Impuesto ecológico activo. Ahora PAGAS por limpiar cadáveres de goblins.", GameManager.soundEventCleanUpCosts);

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= -2;
        HideEventUI();
    }

    public IEnumerator EventSugarRush()
    {
        GameManager.globalSpeedMultiplier *= 2.5f;
        GameManager.globalDamageTakenMultiplier *= 2f;
        ShowEventUI("¡Subidón de azúcar! Los enemigos corren a toda velocidad pero son frágiles como el cristal.", GameManager.soundSad);

        yield return new WaitForSeconds(10f);

        GameManager.globalSpeedMultiplier /= 2.5f;
        GameManager.globalDamageTakenMultiplier /= 2f;
        HideEventUI();
    }

    public IEnumerator EventBossRound()
{
    ShowEventUI("Ha llegado el jefe final... el de prueba, el bueno es DLC.", GameManager.soundBoss);

    yield return new WaitForSeconds(5f);

    HideEventUI();
}

    public IEnumerator EventCoinRain()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-10f, 10f), 10f, 0f);
            yield return new WaitForSeconds(0.1f);
            Instantiate(Resources.Load<GameObject>("prefabCoins"), spawnPos, Quaternion.identity);
        }

        ShowEventUI("Bug detectado: Lluvia de oro. ¡Saca beneficio antes del parche!", null);

        yield return new WaitForSeconds(5f);

        HideEventUI();
    }

    public IEnumerator EventSpawnAds()
    {
        ShowEventUI("Somos un estudio Indie, por favor danos tu dinero.", null);

        GameObject[] adPrefabs = new GameObject[4];
        adPrefabs[0] = Resources.Load<GameObject>("prefabNew");
        adPrefabs[1] = Resources.Load<GameObject>("prefabNew1");
        adPrefabs[2] = Resources.Load<GameObject>("prefabNew2");
        adPrefabs[3] = Resources.Load<GameObject>("prefabNew3");

        Transform parentCanvas = GameObject.Find("Canvas_General").transform;

        for (int i = 0; i < 4; i++)
        {
            GameObject selectedPrefab = adPrefabs[i];
            GameObject spawnedAd = Instantiate(selectedPrefab, parentCanvas, false);
            RectTransform adRect = spawnedAd.GetComponent<RectTransform>();

            float randomX = UnityEngine.Random.Range(-700f, 700f);
            float randomY = UnityEngine.Random.Range(-200f, 200f);

            adRect.anchoredPosition = new Vector2(randomX, randomY);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(5f);

        HideEventUI();
    }

    public void LoadEvents()
    {
        eventList.Add(EventSpawnAds);
        eventList.Add(EventLuckyGold);
        eventList.Add(EventTowerDiscount);
        eventList.Add(EventArcherStrike);
        eventList.Add(EventTaxCollector);
        eventList.Add(EventCleanUpCosts);
        eventList.Add(EventSugarRush);
        eventList.Add(EventCoinRain);
    }
}