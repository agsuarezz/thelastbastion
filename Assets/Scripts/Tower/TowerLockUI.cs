using UnityEngine;

public class TowerLockUI : MonoBehaviour
{
    [Tooltip("Referencia a los datos de la torre para saber si está desbloqueada (allowBuyTower).")]
    public TowerData config;
    [Tooltip("Panel visual (icono o capa oscura) que se activa cuando la torre está bloqueada.")]
    [SerializeField] private GameObject lockedPanel;
    /// <summary>
    /// Método de inicialización de Unity. Evalúa la configuración de la torre 
    /// y activa el panel visual de bloqueo si la compra de la torre no está permitida.
    /// </summary>
    void Start()
    {
        lockedPanel.SetActive(!config.allowBuyTower);
    }
}
