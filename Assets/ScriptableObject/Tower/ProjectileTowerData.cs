using UnityEngine;
[CreateAssetMenu(fileName = "NewProjectileTower", menuName = "Towers/Projectile Tower")]
public class ProjectileTowerData : TowerData
{
    public GameObject projectilePrefab;
    public int baseDamage;
    public float baseFireRate;
}
