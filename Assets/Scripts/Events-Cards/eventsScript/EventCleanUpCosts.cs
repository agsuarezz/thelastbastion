using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewCleanUpCosts", menuName = "Bastion/Events/Clean Up Costs")]

public class EventCleanUpCosts : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalMoneyMultiplier *= -2;
        yield return new WaitForSeconds(10f);
        GameManager.globalMoneyMultiplier /= -2;
    }
}
