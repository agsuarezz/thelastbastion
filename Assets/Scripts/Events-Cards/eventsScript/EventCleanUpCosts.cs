using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewCleanUpCosts", menuName = "Bastion/Events/Clean Up Costs")]
public class EventCleanUpCosts : DynamicEvent
{
    public override IEnumerator Execute()
    {
        int oldMoneyMultiplier = GameManager.globalMoneyMultiplier;

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier * -2;

        yield return new WaitForSeconds(10f);

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier;
    }
}