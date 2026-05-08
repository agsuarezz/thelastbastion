using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewSugarRush", menuName = "Bastion/Events/Sugar Rush")]
public class EventSugarRush : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalSpeedMultiplier *= 2.5f;
        GameManager.globalDamageTakenMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        GameManager.globalSpeedMultiplier /= 2.5f;
        GameManager.globalDamageTakenMultiplier /= 2f;
    }
}
