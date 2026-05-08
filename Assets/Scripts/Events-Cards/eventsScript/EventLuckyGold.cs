using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewLuckyGold", menuName = "Bastion/Events/Lucky Gold")]
public class EventLuckyGold : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalMoneyMultiplier *= 2;
        yield return new WaitForSeconds(10f);
        GameManager.globalMoneyMultiplier /= 2;
    }
}
