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
    private void Start()
    {
        MetaSaveData meta = GameManager.metaProgression;
        textNameTower.text = config.nameOfTower;
        float damage = config.damagePerSecond * meta.upgradesTree[config.nameOfTower][0] * GameManager.globalDamageTakenMultiplier;
        textDamage.text = "Daño: " + damage.ToString("F0");
        float reloadconfig = config.onTime / meta.upgradesTree[config.nameOfTower][2];
        textFireRate.text = "Recarga: " + reloadconfig.ToString("F2") + "/s";
        textAmout.text = "Precio: " + config.upgradeCosts[0].ToString();
    }
}
