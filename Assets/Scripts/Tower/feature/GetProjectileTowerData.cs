using TMPro;
using UnityEngine;

public class GetProjectileTowerData : MonoBehaviour
{
    public ProjectileTowerData config;
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textNameTower;
    public TextMeshProUGUI textDamage;
    public TextMeshProUGUI textFireRate;
    public TextMeshProUGUI textAmout;


    private void Start()
    {
        MetaSaveData meta = GameManager.metaProgression;
        textNameTower.text = config.nameOfTower;
        float realDamage = config.baseDamage * meta.upgradesTree[config.nameOfTower][0];
        textDamage.text = "Daño: " + realDamage.ToString("F0");
        float realFireRate = (config.baseFireRate / meta.upgradesTree[config.nameOfTower][2]) * GameManager.globalAttackSpeedMultiplier;
        textFireRate.text = "Recarga: " + realFireRate.ToString("F2") + "/s";
        textAmout.text = "Precio: " +  config.upgradeCosts[0].ToString();
    }
}
