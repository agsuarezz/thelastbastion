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
        // 4. Activamos o desactivamos el candado
        lockedPanel.SetActive(!IsTowerUnlocked(config));
    }
    public static bool IsTowerUnlocked(TowerData config)
    {
        // 1. Cargamos el archivo de progreso real
        MetaSaveData meta = SaveSystem.LoadMeta();

        // 2. Por defecto, asumimos que está desbloqueada (para las torres básicas)
        bool isUnlocked = true;

        // 3. Comprobamos si es una torre especial mirando su nombre
        if (config != null)
        {
            switch (config.nameOfTower)
            {
                case "Torre Soporte":
                    isUnlocked = meta.isSupportTowerUnlocked;
                    break;
                case "Torre Infernal":
                    isUnlocked = meta.isInfernalTowerUnlocked;
                    break;
            }
        }
        return isUnlocked;
    }
}
