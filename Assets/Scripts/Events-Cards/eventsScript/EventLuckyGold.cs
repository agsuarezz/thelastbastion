using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewLuckyGold", menuName = "Bastion/Events/Lucky Gold")]
public class EventLuckyGold : DynamicEvent
{
    public override IEnumerator Execute()
    {
        int oldMoneyMultiplier = GameManager.globalMoneyMultiplier;

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier * 10;

        yield return new WaitForSeconds(5f);

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier;
    }
}