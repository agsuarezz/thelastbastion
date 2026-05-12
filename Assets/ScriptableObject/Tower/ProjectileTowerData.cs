using UnityEngine;
[CreateAssetMenu(fileName = "NewProjectileTower", menuName = "Towers/Projectile Tower")]
public class ProjectileTowerData : TowerData
{
    public GameObject projectilePrefab;
    public float baseDamage;
    public float baseFireRate;
}
