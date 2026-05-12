using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Towers/TowerData")]
public abstract class TowerData : ScriptableObject
{
    public string nameOfTower;
    public float baseAttackRadius;
    public float damageUpgradeAmount;
    public float cooldownUpgradeAmount;
    public float[] upgradeCosts;
}