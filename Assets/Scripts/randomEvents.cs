using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.VisualScripting.FlowStateWidget;

public class randomEvents : MonoBehaviour
{
    public static List<System.Func<IEnumerator>> eventList;
    public TextMeshProUGUI messageEvent;
    private void Start()
    {
        eventList = new List<System.Func<IEnumerator>>();
        eventList.Add(EventGoblinStampede);
        eventList.Add(EventLuckyGold);
    }
    public IEnumerator EventGoblinStampede()
    {
        Enemy.globalSpeedMultiplier = 2f;
        messageEvent.text = "Error 404: Exceso de cafeína detectado en el enemigo.";
        yield return new WaitForSeconds(5f);
        Enemy.globalSpeedMultiplier = 1f;
        messageEvent.text = "";
    }
    public IEnumerator EventLuckyGold()
    {
        GameManager.globalMoneyMultiplier = 2;
        messageEvent.text = "Hacienda te ha devuelto la declaración. ¡Disfruta el bonus!";
        yield return new WaitForSeconds(10f);
        GameManager.globalMoneyMultiplier = 1;
        messageEvent.text = "";
    }
}
