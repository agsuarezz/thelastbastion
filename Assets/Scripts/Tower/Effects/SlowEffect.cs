using System.Collections;
using UnityEngine;

/// <summary>
/// Componente que aplica una ralentización temporal al enemigo.
/// Se añade dinámicamente por SlowOnHitEffect. Si se aplica mientras ya
/// está activo, simplemente refresca el tiempo restante al valor más alto.
/// </summary>
public class SlowEffect : MonoBehaviour
{
    // ── Estado interno ────────────────────────────────────────────────────────

    private Enemy   _enemy;
    private float   _activeMultiplier = 1f;   // multiplicador actualmente aplicado
    private bool    _isSlowed         = false;
    private Coroutine _slowCoroutine  = null;

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica (o refresca) el slow sobre el enemigo.
    /// </summary>
    /// <param name="speedMultiplier">Fracción de velocidad: 0.5 = mitad de velocidad.</param>
    /// <param name="duration">Segundos que dura el efecto.</param>
    public void ApplySlow(float speedMultiplier, float duration)
    {
        if (_enemy == null)
            _enemy = GetComponent<Enemy>();

        if (_enemy == null || _enemy.IsDead) return;

        // Si ya hay un slow activo, paramos la corrutina anterior para refrescarla
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
            // Si el multiplicador nuevo es más fuerte, lo usamos; si no, mantenemos el que había
            if (speedMultiplier < _activeMultiplier)
                _activeMultiplier = speedMultiplier;
        }
        else
        {
            _activeMultiplier = speedMultiplier;
        }

        _isSlowed = true;
        _slowCoroutine = StartCoroutine(SlowRoutine(duration));
    }

    // ── Lógica interna ────────────────────────────────────────────────────────

    private IEnumerator SlowRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _isSlowed         = false;
        _activeMultiplier = 1f;
        _slowCoroutine    = null;
    }

    // ── Propiedad de velocidad ────────────────────────────────────────────────

    /// <summary>
    /// Multiplicador de velocidad local producido por este efecto.
    /// Enemy.cs lo debe multiplicar en su cálculo de movimiento.
    /// </summary>
    public float SpeedMultiplier => _isSlowed ? _activeMultiplier : 1f;

    private void OnDisable()
    {
        // Al desactivar/destruir limpiamos el estado
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
            _slowCoroutine = null;
        }
        _isSlowed         = false;
        _activeMultiplier = 1f;
    }
}
