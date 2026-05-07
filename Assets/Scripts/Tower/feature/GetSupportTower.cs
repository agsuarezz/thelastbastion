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
        textNameTower.text = config.nameOfTower;
        textExtraDamage.text = "Daño Extra: +" + config.baseIncreaseDamage.ToString();
        textScanRate.text = "Escaneo: " + config.baseFireRate.ToString() + "s";
        textPrice.text = "Precio: " + config.upgradeCosts[0].ToString();
    }
}