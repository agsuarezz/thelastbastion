using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Towers/TowerData")]
public class TowerData : ScriptableObject
{
    public string nameOfTower;
    public float baseAttackRadius;
    public float baseFireCooldown;
    public int baseDamage;
    // ==========================================
    // DICCIONARIO DE MEJORAS (0: Mediana, 1: Ligera, 2: Pesada)
    // ==========================================
    // Cuánto daño gana cada torre al subir de nivel:
    public int damageUpgradeAmount;
    public float cooldownUpgradeAmount;
    public int[] upgradeCosts;
}