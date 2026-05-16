using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewDenseFog", menuName = "Bastion/Events/Dense Fog")]
public class EventDenseFog : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float oldRadiusMultiplier = GameManager.globalRadiusMultiplier;

        GameManager.globalRadiusMultiplier = oldRadiusMultiplier * 0.5f;

        yield return new WaitForSeconds(10f);

        GameManager.globalRadiusMultiplier = oldRadiusMultiplier;
    }
}