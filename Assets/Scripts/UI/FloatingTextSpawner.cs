using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private FloatingText floatingTextPrefab;

    [Tooltip("Canvas donde se instancian los textos.")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Posición de spawn")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 80f);
    [SerializeField] private float randomSpread = 30f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (floatingTextPrefab == null)
            Debug.LogError("[FloatingTextSpawner] ¡Falta asignar el Prefab!");
    }

    public void Show(int amount, bool isGain)
    {
        // LOG TEMPORAL: muestra el valor exacto que llega
        Debug.Log($"[FloatingText] Show → amount={amount} isGain={isGain}");

        if (floatingTextPrefab == null || targetCanvas == null) return;

        FloatingText instance = Instantiate(floatingTextPrefab, targetCanvas.transform);

        RectTransform rt = instance.GetComponent<RectTransform>();
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomSpread, randomSpread),
            Random.Range(-randomSpread * 0.5f, randomSpread * 0.5f)
        );
        // Aplicamos el offset ANTES de Initialize para que la animación parta del sitio correcto
        rt.anchoredPosition = spawnOffset + randomOffset;

        instance.Initialize(amount, isGain);
    }
}