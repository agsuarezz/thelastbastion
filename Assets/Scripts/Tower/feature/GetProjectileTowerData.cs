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
    private MetaSaveData meta;

    private void Start()
    {
        meta = SaveSystem.LoadMeta();
    }

    // Update is called once per frame
    void Update()
    {
        textNameTower.text = config.nameOfTower;
        textDamage.text = "Daño: " + ((int)(config.baseDamage * meta.upgradesTree[config.nameOfTower][0]) * GameManager.globalDamageTakenMultiplier).ToString();
        textFireRate.text = "Recarga: " +  (config.baseFireRate / meta.upgradesTree[config.nameOfTower][2] * GameManager.globalAttackSpeedMultiplier).ToString("F2") + "/s";
        textAmout.text = "Precio: " +  config.upgradeCosts[0].ToString();
    }
}
