using UnityEngine;

public class TowerLockUI : MonoBehaviour
{
    public TowerData config;
    [SerializeField] private GameObject lockedPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lockedPanel.SetActive(!config.allowBuyTower);
    }
}
