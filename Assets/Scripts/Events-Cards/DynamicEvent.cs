using UnityEngine;
using System.Collections;
public enum EventType
{
    Beneficial, // Eventos buenos
    Harmful // Eventos malos
}
[CreateAssetMenu(fileName = "NewDynamicEvent", menuName = "Bastion/Dynamic Event")]
public abstract class DynamicEvent : ScriptableObject
{
    [Header("Información General")]
    public string eventName;
    [TextArea] public string description;
    [Header("Configuración de Pesos")]
    public EventType type;
    public AudioClip audioClip;
    public float baseWeight = 10f;
    public abstract IEnumerator Execute();
}
