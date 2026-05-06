using TMPro;
using UnityEngine;

public class GetProjectileTowerData : MonoBehaviour
{
    public ProjectileTowerData config;
    public TextMeshProUGUI textDamage;
    public TextMeshProUGUI textFireRate;
    public TextMeshProUGUI textAmout;
    // Update is called once per frame
    void Update()
    {
        textDamage.text = "Daño: " + config.baseDamage.ToString();
        textFireRate.text = "Recarga: " +  config.baseFireRate.ToString() + "/s";
        textAmout.text = "Precio: " +  config.upgradeCosts[0].ToString();
    }
}
