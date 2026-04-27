using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    [Header("Objetivos")]
    [Tooltip("El objeto del mapa que queremos perseguir (Ej: El Castillo)")]
    public Transform target;

    [Tooltip("Tu cámara principal (Normalmente Camera.main)")]
    private Camera mainCamera;

    [Header("Ajustes visuales")]
    [Tooltip("Desfase en píxeles. Útil si quieres que el texto salga un poco más arriba del castillo y no en el centro.")]
    public Vector3 offset;

    private RectTransform uiElement;

    private void Start()
    {
        mainCamera = Camera.main;
        uiElement = GetComponent<RectTransform>();
    }

    // Usamos LateUpdate en vez de Update. 
    // Así nos aseguramos de que el Canvas se mueva DESPUÉS de que la cámara o el objeto se hayan movido.
    private void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        // 1. Traducimos la coordenada del mundo a coordenadas de píxeles de la pantalla
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position);

        // 2. Le sumamos el desfase por si quieres que flote por encima
        screenPosition += offset;

        // 3. Movemos el elemento del Canvas (Al ser Overlay, su .position acepta píxeles directamente)
        uiElement.position = screenPosition;
    }
}