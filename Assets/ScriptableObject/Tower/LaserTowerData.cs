using UnityEngine;

[CreateAssetMenu(fileName = "NewLaserTower", menuName = "Towers/Laser Tower")]
public class LaserTowerData : TowerData
{
    public float damagePerSecond;
    public float onTime;  // Los 5 segundos encendido
    public float offTime; // Los 2 segundos apagado
}