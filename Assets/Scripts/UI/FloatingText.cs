using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla la animación de un texto flotante temporal en la UI.
/// Adjunta este script al Prefab de texto (que debe tener un componente TextMeshProUGUI).
/// Se auto-destruye al terminar la animación.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingText : MonoBehaviour
{
    // ─── Configuración de la animación ───────────────────────────────────────
    [Header("Animación")]
    [Tooltip("Duración total de vida del texto en segundos.")]
    [SerializeField] private float lifetime = 1.4f;

    [Tooltip("Distancia en píxeles que el texto asciende durante su vida.")]
    [SerializeField] private float floatDistance = 60f;

    [Tooltip("Curva que controla la velocidad del movimiento ascendente (opcional).")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Fracción de la vida (0-1) a partir de la cual empieza el fade-out.")]
    [SerializeField] [Range(0f, 1f)] private float fadeStartFraction = 0.4f;

    // ─── Referencias internas ─────────────────────────────────────────────────
    private TextMeshProUGUI _label;
    private RectTransform   _rect;
    private Vector2         _startAnchoredPosition;

    // ─── Colores predefinidos ─────────────────────────────────────────────────
    private static readonly Color ColorGain = new Color(0.20f, 0.85f, 0.20f); // Verde
    private static readonly Color ColorLoss = new Color(0.95f, 0.20f, 0.20f); // Rojo

    // =========================================================================
    //  API PÚBLICA
    // =========================================================================

    /// <summary>
    /// Inicializa el texto flotante con la cantidad y el tipo de transacción.
    /// Llama a este método justo después de instanciar el prefab.
    /// </summary>
    /// <param name="amount">Valor absoluto de la cantidad (positivo siempre).</param>
    /// <param name="isGain">True = ganancia (verde / +), False = gasto (rojo / -).</param>
    public void Initialize(int amount, bool isGain)
    {
        _label = GetComponent<TextMeshProUGUI>();
        _rect  = GetComponent<RectTransform>();
        _startAnchoredPosition = _rect.anchoredPosition;

        _label.text  = isGain ? $"+ {amount}" : $"- {amount}";
        _label.color = isGain ? ColorGain : ColorLoss;

        StartCoroutine(AnimateAndDestroy());
    }

    // =========================================================================
    //  CORRUTINA DE ANIMACIÓN
    // =========================================================================

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;

        // Guardamos el color inicial para no perder el canal alfa original
        Color baseColor = _label.color;

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime; // 0 → 1 a lo largo de toda la vida

            // — Movimiento ascendente —
            float curvedT = moveCurve.Evaluate(t);
            _rect.anchoredPosition = _startAnchoredPosition + Vector2.up * (floatDistance * curvedT);

            // — Fade-out —
            float alpha;
            if (t < fadeStartFraction)
            {
                alpha = 1f; // Completamente visible en el tramo inicial
            }
            else
            {
                // Interpola de 1 → 0 durante el tramo de desvanecimiento
                float fadeT = (t - fadeStartFraction) / (1f - fadeStartFraction);
                alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            _label.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null; // Espera un frame
        }

        Destroy(gameObject);
    }
}
