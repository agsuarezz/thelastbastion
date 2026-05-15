using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "NewArcherStrike", menuName = "Bastion/Events/Archer Strike")]

public class EventArcherStrike : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalAttackSpeedMultiplier *= 1000f;
        yield return new WaitForSeconds(5f);
        GameManager.globalAttackSpeedMultiplier /= 1000f;
    }
}
