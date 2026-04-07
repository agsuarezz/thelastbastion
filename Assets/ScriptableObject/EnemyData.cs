using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float health;
    public int damage;
    public float speed;
    public float speedDefault;
    void Start()
    {
        speedDefault = speed;
    }
}
