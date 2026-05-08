using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewTaxCollector", menuName = "Bastion/Events/Tax Collector")]
public class EventTaxCollector : DynamicEvent
{
    public override IEnumerator Execute()
    {
        int taxes = (int)(GameManager.countMoney * 0.40f);
        GameManager.countMoney -= taxes;
        yield return new WaitForSeconds(5f);
    }
}
