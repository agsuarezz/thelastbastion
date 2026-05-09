using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewBossRound", menuName = "Bastion/Events/Boss Round")]

public class EventBossRound : DynamicEvent
{
    public override IEnumerator Execute()
    {
        GameManager.hasBossAppeared = true;
        yield break;
    }
}
