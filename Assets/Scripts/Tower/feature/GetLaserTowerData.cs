using TMPro;
using UnityEngine;

public class GetLaserTowerData : MonoBehaviour
{
    public LaserTowerData config;
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textNameTower;
    public TextMeshProUGUI textDamage;
    public TextMeshProUGUI textFireRate;
    public TextMeshProUGUI textAmout;
    // Update is called once per frame
    void Update()
    {
        textNameTower.text = config.nameOfTower;
        textDamage.text = "Daño: " + config.damagePerSecond.ToString();
        textFireRate.text = "Recarga: " + config.onTime.ToString() + "/s";
        textAmout.text = "Precio: " + config.upgradeCosts[0].ToString();
    }
}
