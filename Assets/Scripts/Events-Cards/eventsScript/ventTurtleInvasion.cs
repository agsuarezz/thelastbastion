using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewTurtleInvasion", menuName = "Bastion/Events/Turtle Invasion")]
public class EventTurtleInvasion : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float oldSpeedMultiplier = GameManager.globalSpeedMultiplier;
        float oldHealthMultiplier = GameManager.globalEnemyHealthMultiplier;

        GameManager.globalSpeedMultiplier = oldSpeedMultiplier * 0.3f;
        GameManager.globalEnemyHealthMultiplier = oldHealthMultiplier * 3f;

        yield return new WaitForSeconds(10f);

        GameManager.globalSpeedMultiplier = oldSpeedMultiplier;
        GameManager.globalEnemyHealthMultiplier = oldHealthMultiplier;
    }
}