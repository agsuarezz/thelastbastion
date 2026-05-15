using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewTurtleInvasion", menuName = "Bastion/Events/Turtle Invasion")]
public class EventTurtleInvasion : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.globalSpeedMultiplier *= 0.3f;
        GameManager.globalEnemyHealthMultiplier *= 3f;

        yield return new WaitForSeconds(10f); // Este puede durar 10s porque es neutral

        GameManager.globalSpeedMultiplier /= 0.3f;
        GameManager.globalEnemyHealthMultiplier /= 3f;
    }
}