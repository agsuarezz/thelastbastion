using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewTaxCollector", menuName = "Bastion/Events/Tax Collector")]
public class EventTaxCollector : DynamicEvent
{
    [Header("Configuración del Robo")]
    [Tooltip("Cantidad de oro EXACTA que te quita. Pon 0 si prefieres usar el multiplicador.")]
    public int fixedGoldToRemove = 0;

    [Tooltip("Porcentaje de oro que quita (1 = 100%, 0.5 = 50%). Solo funciona si fixedGoldToRemove es 0.")]
    public float theftMultiplier = 1f;

    public override IEnumerator Execute()
    {
        // Si le hemos puesto un valor fijo (ej: 1 moneda), quitamos eso
        if (fixedGoldToRemove > 0)
        {
            GameManager.countMoney -= fixedGoldToRemove;
        }
        // Si no, usamos el porcentaje (ej: 1f para quitarlo todo)
        else
        {
            int taxes = Mathf.RoundToInt(GameManager.countMoney * theftMultiplier);
            GameManager.countMoney -= taxes;
        }

        yield return new WaitForSeconds(5f);
    }
}