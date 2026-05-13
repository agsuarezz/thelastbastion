using TMPro;
using UnityEngine;

public class GetSupportTower : MonoBehaviour
{
    public SupportTowerData config;

    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textNameTower;
    public TextMeshProUGUI textExtraDamage;
    public TextMeshProUGUI textScanRate;
    public TextMeshProUGUI textPrice;

    void Start()
    {
        MetaSaveData meta = GameManager.metaProgression;
        textNameTower.text = config.nameOfTower;
        float damage = config.baseIncreaseDamage * meta.upgradesTree[config.nameOfTower][0];
        textExtraDamage.text = "Daño Extra: +" + damage.ToString("F0");
        float checkTime = config.baseFireRate / meta.upgradesTree[config.nameOfTower][2];
        textScanRate.text = "Escaneo: " + checkTime.ToString("F2") + "s";
        textPrice.text = "Precio: " + config.upgradeCosts[0].ToString();
    }
}