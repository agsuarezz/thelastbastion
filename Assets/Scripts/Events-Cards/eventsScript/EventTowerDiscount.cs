using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewTowerDiscount", menuName = "Bastion/Events/Tower Discount")]
public class EventTowerDiscount : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalCostMultiplier *= 0.5f;
        yield return new WaitForSeconds(10f);
        GameManager.globalCostMultiplier /= 0.5f;
    }
}
