using UnityEngine;

[CreateAssetMenu(fileName = "NewShortCut", menuName = "Bastion/ShortCut")]
public class ShortCutScript : ScriptableObject
{
    [Header("Atajos de Construcción (Torres)")]
    public KeyCode keyTowerMedian = KeyCode.Alpha1;
    public KeyCode keyTowerLight = KeyCode.Alpha2;
    public KeyCode keyTowerHeavy = KeyCode.Alpha3;
    public KeyCode keyTowerInfernal = KeyCode.Alpha4;
    public KeyCode keyTowerSupport = KeyCode.Alpha5;

    [Space(10)]

    [Header("Controles de la Partida")]
    [Tooltip("Tecla para saltar a la siguiente ronda rápidamente")]
    public KeyCode keyToPassRound = KeyCode.Space;

    [Tooltip("Tecla para acelerar o pausar el tiempo del juego")]
    public KeyCode keyToControlVelocityGame = KeyCode.N;

    [Tooltip("Tecla para vender la torre que esté seleccionada actualmente")]
    public KeyCode keyToSellTower = KeyCode.V;
    [Tooltip("Tecla para pausar o reanudar la partida actual.")]
    public KeyCode keyToPauseGame = KeyCode.Escape;
}