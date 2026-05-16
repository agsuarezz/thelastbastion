using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArcherStrike", menuName = "Bastion/Events/Archer Strike")]
public class EventArcherStrike : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float oldAttackSpeedMultiplier = GameManager.globalAttackSpeedMultiplier;
        float oldRadiusMultiplier = GameManager.globalRadiusMultiplier;

        GameManager.globalRadiusMultiplier = 0f;
        GameManager.globalAttackSpeedMultiplier = 10f;

        yield return new WaitForSeconds(5f);

        GameManager.globalRadiusMultiplier = oldRadiusMultiplier;
        GameManager.globalAttackSpeedMultiplier = oldAttackSpeedMultiplier;
    }
}