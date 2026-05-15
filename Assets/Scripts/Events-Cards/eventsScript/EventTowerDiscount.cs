using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewTowerDiscount", menuName = "Bastion/Events/Tower Discount")]
public class EventTowerDiscount : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float lastMultiplier = GameManager.globalCostMultiplier;
        GameManager.globalCostMultiplier = 0f;
        yield return new WaitForSeconds(5f);
        GameManager.globalCostMultiplier = lastMultiplier;
    }
}
