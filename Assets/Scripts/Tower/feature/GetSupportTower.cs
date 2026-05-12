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
    private MetaSaveData meta;

    void Start()
    {
        meta = SaveSystem.LoadMeta();
        textNameTower.text = config.nameOfTower;
        textExtraDamage.text = "Daño Extra: +" + ((int)(config.baseIncreaseDamage * meta.upgradesTree[config.nameOfTower][0])).ToString();
        textScanRate.text = "Escaneo: " + (config.baseFireRate / meta.upgradesTree[config.nameOfTower][2]).ToString("F2") + "s";
        textPrice.text = "Precio: " + config.upgradeCosts[0].ToString();
    }
}