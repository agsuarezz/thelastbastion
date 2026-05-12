using System;
using UnityEngine;
public enum tower
{
    Media,
    Ligera,
    Pesada,
    Infernal,
    Soporte
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Bastion/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Info Básica")]
    public string skillID;
    public string skillName;
    public Sprite icon;

    [Header("Reglas de Nivel")]
    public int maxNBuy = 1;      // ¿Cuántas veces se puede comprar?
    public int baseCost;    // Coste del primer nivel
    public int costMultiplier; // Cuánto se encarece cada nivel

    [Header("Beneficio")]
    [Tooltip("El valor que suma por cada nivel comprado. Ej: 0.2 para un 20%")]
    public float benefitPerLevel = 0.2f;
    public int typeUpgrade = 1;
    public tower typeTower;

    [Header("Reglas")]
    public SkillData prerequisite;
}
