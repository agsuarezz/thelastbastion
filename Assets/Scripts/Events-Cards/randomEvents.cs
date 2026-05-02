using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Main manager for random game events.
/// Handles the event catalog (positive and negative) that temporarily alters gameplay rules.
/// </summary>
public class randomEvents : MonoBehaviour
{
    [Header("Event Configuration")]
    public static List<System.Func<IEnumerator>> eventList;

    [Tooltip("UI Text component for displaying event messages.")]
    public TextMeshProUGUI messageEventText;

    [Tooltip("Background panel/image for the event messages.")]
    public GameObject eventBackgroundUI;

    private void Start()
    {
        eventList = new List<System.Func<IEnumerator>>();
        LoadEvents();
        HideEventUI(); // Ensure UI is clean at start
    }

    // 🌟 HELPER FUNCTIONS (The "Don't Repeat Yourself" part)

    private void ShowEventUI(string message, AudioClip soundEffect)
    {
        if (messageEventText != null) messageEventText.text = message;
        if (eventBackgroundUI != null) eventBackgroundUI.SetActive(true);
        if (soundEffect != null) GameManager.sound(soundEffect);
    }

    private void HideEventUI()
    {
        if (messageEventText != null) messageEventText.text = "";
        if (eventBackgroundUI != null) eventBackgroundUI.SetActive(false);
    }

    // --- RANDOM EVENTS ---

    public IEnumerator EventLuckyGold()
    {
        GameManager.globalMoneyMultiplier *= 2;
        ShowEventUI("The goblins just got paid and their pockets are full. Get them!", GameManager.soundHappy);

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= 2;
        HideEventUI();
    }

    public IEnumerator EventTowerDiscount()
    {
        GameManager.globalCostMultiplier *= 0.5f;
        ShowEventUI("Black Friday at the smithy! Towers are half price.", GameManager.soundHappy);

        yield return new WaitForSeconds(10f);

        GameManager.globalCostMultiplier /= 0.5f;
        HideEventUI();
    }

    public IEnumerator EventArcherStrike()
    {
        GameManager.globalAttackSpeedMultiplier *= 4f;
        ShowEventUI("Archer union on strike. Attack speed significantly reduced.", GameManager.soundSad);

        yield return new WaitForSeconds(7f);

        GameManager.globalAttackSpeedMultiplier /= 4f;
        HideEventUI();
    }

    public IEnumerator EventTaxCollector()
    {
        int taxes = (int)(GameManager.countMoney * 0.40f);
        GameManager.countMoney -= taxes;
        ShowEventUI($"Tax inspector confiscated {taxes} Gold for undeclared defensive structures.", GameManager.soundPay);

        yield return new WaitForSeconds(5f);

        HideEventUI();
    }

    public IEnumerator EventCleanUpCosts()
    {
        GameManager.globalMoneyMultiplier *= -2;
        ShowEventUI("Ecological tax active. You now PAY to clean up goblin corpses.", GameManager.soundEventCleanUpCosts);

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier /= -2;
        HideEventUI();
    }

    public IEnumerator EventSugarRush()
    {
        GameManager.globalSpeedMultiplier *= 2.5f;
        GameManager.globalDamageTakenMultiplier *= 2f;
        ShowEventUI("Sugar rush! Enemies are sprinting but they're fragile as glass.", GameManager.soundSad);

        yield return new WaitForSeconds(10f);

        GameManager.globalSpeedMultiplier /= 2.5f;
        GameManager.globalDamageTakenMultiplier /= 2f;
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

        ShowEventUI("Bug detected: Raining gold. Profit before the patch!", null);

        yield return new WaitForSeconds(5f);

        HideEventUI();
    }

    public IEnumerator EventSpawnAds()
    {
        ShowEventUI("We are an Indie studio, please give us your money.", null);

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