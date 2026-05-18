using UnityEngine;

public class MoneyNotificationManager : MonoBehaviour
{
    public static MoneyNotificationManager Instance;

    [SerializeField] private MoneyNotification notificationPrefab;
    [SerializeField] private Transform notificationsParent;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(int amount, bool isGain)
    {
        MoneyNotification notification =
            Instantiate(notificationPrefab, notificationsParent);

        notification.Setup(amount, isGain);
    }
}