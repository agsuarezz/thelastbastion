using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.FlowStateWidget;

public class randomEvents : MonoBehaviour
{
    public static List<System.Func<IEnumerator>> eventList;
    private void Start()
    {
        eventList = new List<System.Func<IEnumerator>>();
        eventList.Add(EventGoblinStampede);
    }
    public IEnumerator EventGoblinStampede()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            float speed = enemy.GetComponent<EnemyData>().speed;
            enemy.GetComponent<EnemyData>().speed = speed * 1.5f; 
        }
        yield return new WaitForSeconds(5f);
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyData>().speed = enemy.GetComponent<EnemyData>().speedDefault;
        }
    }
}
