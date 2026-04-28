using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Towers/TowerData")]
public class TowerData : ScriptableObject
{
    public string nameOfTower;
    public float baseAttackRadius;
    public float baseFireCooldown;
    public int baseDamage;
}