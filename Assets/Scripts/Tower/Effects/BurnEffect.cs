using System.Collections;
using UnityEngine;

/// <summary>
/// Efecto de quemadura (DoT de fuego). Se añade como componente temporal
/// al enemigo al impactar. Si el enemigo ya está ardiendo, reinicia el timer
/// en lugar de apilar instancias.
/// </summary>
public class BurnEffect : MonoBehaviour, IOnHitEffect
{
    // ── Parámetros de la quemadura ──────────────────────────────────────────
    private int   _damagePerTick;   // Daño por tick
    private float _tickInterval;    // Segundos entre ticks
    private int   _totalTicks;      // Número de ticks totales

    private Coroutine _burnCoroutine;

    // ── Debug visual ─────────────────────────────────────────────────────────
    // Actívalo desde el Inspector o con el script BurnDebugToggle.
    // Se elimina automáticamente junto con el efecto.
#if UNITY_EDITOR
    private GameObject _debugSquare;

    private void ShowDebugSquare()
    {
        if (_debugSquare != null) return;

        _debugSquare = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _debugSquare.name = "[DEBUG] BurnIndicator";

        // Quitar el collider para que no interfiera con el juego
        Destroy(_debugSquare.GetComponent<Collider>());
        Destroy(_debugSquare.GetComponent<MeshCollider>());

        // Tamaño y posición: cuadrado grande centrado sobre el enemigo
        _debugSquare.transform.SetParent(transform);
        _debugSquare.transform.localPosition = Vector3.zero;
        _debugSquare.transform.localScale    = new Vector3(2f, 2f, 1f);

        // Color naranja semitransparente
        var renderer  = _debugSquare.GetComponent<Renderer>();
        var material  = new Material(Shader.Find("Sprites/Default"));
        material.color = new Color(1f, 0.4f, 0f, 0.55f);
        renderer.material = material;
    }

    private void HideDebugSquare()
    {
        if (_debugSquare != null)
            Destroy(_debugSquare);
    }
#endif

    // ── IOnHitEffect ────────────────────────────────────────────────────────

    /// <summary>
    /// Punto de entrada llamado por Projectile al impactar.
    /// Si el enemigo ya arde, reinicia el efecto en lugar de apilar.
    /// </summary>
    public void Apply(Enemy enemy)
    {
        if (_burnCoroutine != null)
        {
            StopCoroutine(_burnCoroutine);
        }
#if UNITY_EDITOR
        ShowDebugSquare();
#endif
        _burnCoroutine = StartCoroutine(BurnRoutine(enemy));
    }

    // ── Configuración ────────────────────────────────────────────────────────

    /// <summary>
    /// Inyecta los parámetros del efecto desde fuera (llamado por Projectile).
    /// Esto evita acoplamiento directo al GameManager desde aquí.
    /// </summary>
    public void Configure(int damagePerTick, float tickInterval, int totalTicks)
    {
        _damagePerTick = damagePerTick;
        _tickInterval  = tickInterval;
        _totalTicks    = totalTicks;
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private IEnumerator BurnRoutine(Enemy enemy)
    {
        for (int i = 0; i < _totalTicks; i++)
        {
            yield return new WaitForSeconds(_tickInterval);

            if (enemy == null || enemy.IsDead)
            {
#if UNITY_EDITOR
                HideDebugSquare();
#endif
                yield break;
            }

            enemy.TakeDamage(_damagePerTick);
        }

#if UNITY_EDITOR
        HideDebugSquare();
#endif
        // El efecto terminó: nos auto-destruimos del GameObject del enemigo
        Destroy(this);
    }
}