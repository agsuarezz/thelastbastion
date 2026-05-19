using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "NewCleanUpCosts", menuName = "Bastion/Events/Clean Up Costs")]
public class EventCleanUpCosts : DynamicEvent
{
    public override IEnumerator Execute()
    {
        int oldMoneyMultiplier = GameManager.globalMoneyMultiplier;

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier * -2;

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if(gameManager != null && gameManager.spawner != null)
        {
            // 1. ESPERAMOS A QUE EMPIECE LA RONDA (statusRound == false)
            // Así evitamos que el evento se cancele en el milisegundo en que se crea
            yield return new WaitUntil(() => gameManager.spawner.statusRound() == false);
            // El código se queda "congelado" en esta línea HASTA que statusRound devuelva true
            yield return new WaitUntil(() => gameManager.spawner.statusRound() == true);
            // Le damos un margen para que resetee
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(15f);
        }

        GameManager.globalMoneyMultiplier = oldMoneyMultiplier;
    }
}