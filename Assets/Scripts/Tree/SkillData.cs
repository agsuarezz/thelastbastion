using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Bastion/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Info Básica")]
    public string skillID;
    public string skillName;
    public int cost;
    public Sprite icon;
    public bool buyed;

    [Header("Reglas")]
    public SkillData prerequisite;
}
