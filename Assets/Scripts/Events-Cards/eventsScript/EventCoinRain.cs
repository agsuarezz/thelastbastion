using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "NewCoinRain", menuName = "Bastion/Events/Coin Rain")]
public class EventCoinRain : DynamicEvent
{
    [Header("Configuración de la Lluvia")]
    [Tooltip("Número de monedas que van a caer")]
    public int coinAmount = 5;
    public override IEnumerator Execute()
    {
        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-10f, 10f), 10f, 0f);
            yield return new WaitForSeconds(0.1f);
            Instantiate(Resources.Load<GameObject>("prefabCoins"), spawnPos, Quaternion.identity);
        }
    }
}
