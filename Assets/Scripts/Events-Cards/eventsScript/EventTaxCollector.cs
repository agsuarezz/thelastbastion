using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewTaxCollector", menuName = "Bastion/Events/Tax Collector")]
public class EventTaxCollector : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.countMoney = 0;
        yield return new WaitForSeconds(5f);
    }
}
