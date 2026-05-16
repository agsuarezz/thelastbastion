using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewSugarRush", menuName = "Bastion/Events/Sugar Rush")]
public class EventSugarRush : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float oldSpeedMultiplier = GameManager.globalSpeedMultiplier;
        float oldDamageTakenMultiplier = GameManager.globalDamageTakenMultiplier;

        GameManager.globalSpeedMultiplier = oldSpeedMultiplier * 2.5f;
        GameManager.globalDamageTakenMultiplier = oldDamageTakenMultiplier * 2f;

        yield return new WaitForSeconds(10f);

        GameManager.globalSpeedMultiplier = oldSpeedMultiplier;
        GameManager.globalDamageTakenMultiplier = oldDamageTakenMultiplier;
    }
}