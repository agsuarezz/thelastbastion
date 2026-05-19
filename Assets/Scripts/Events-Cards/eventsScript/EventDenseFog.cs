using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewDenseFog", menuName = "Bastion/Events/Dense Fog")]
public class EventDenseFog : DynamicEvent
{
    public override IEnumerator Execute()
    {
        float oldRadiusMultiplier = GameManager.globalRadiusMultiplier;

        GameManager.globalRadiusMultiplier = oldRadiusMultiplier * 0.5f;

        GameManager gm = FindAnyObjectByType<GameManager>();

        if (gm != null && gm.spawner != null)
        {
            // 1. ESPERAMOS A QUE EMPIECE LA RONDA (statusRound == false)
            // Así evitamos que el evento se cancele en el milisegundo en que se crea
            yield return new WaitUntil(() => gm.spawner.statusRound() == false);
            // El código se queda "congelado" en esta línea HASTA que statusRound devuelva true
            yield return new WaitUntil(() => gm.spawner.statusRound() == true);
            // Le damos un margen para que resetee
            yield return new WaitForSeconds(1f);
        }
        else
        {
            // Paracaídas de seguridad por si acaso se nos rompe el GameManager
            yield return new WaitForSeconds(15f);
        }

        GameManager.globalRadiusMultiplier = oldRadiusMultiplier;
    }
}