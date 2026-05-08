using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "NewArcherStrike", menuName = "Bastion/Events/Archer Strike")]

public class EventArcherStrike : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalAttackSpeedMultiplier *= 4f;
        yield return new WaitForSeconds(7f);
        GameManager.globalAttackSpeedMultiplier /= 4f;
    }
}
