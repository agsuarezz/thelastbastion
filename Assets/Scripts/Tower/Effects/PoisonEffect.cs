using System.Collections;
using UnityEngine;

/// <summary>
/// Efecto de veneno (DoT apilable). A diferencia del fuego, cada impacto
/// añade un stack que incrementa el daño por tick. Los stacks comparten
/// un único timer que se reinicia con cada nueva aplicación.
///
/// Stacking: damagePerTick = baseDamagePerTick * currentStacks
/// Límite de stacks configurable para evitar daños desorbitados.
/// </summary>
public class PoisonEffect : MonoBehaviour
{
    // ── Parámetros inyectados ────────────────────────────────────────────────
    private int   _baseDamagePerTick;
    private float _tickInterval;
    private int   _totalTicks;
    private int   _maxStacks;

    // ── Estado interno ───────────────────────────────────────────────────────
    private int       _currentStacks;
    private Coroutine _poisonCoroutine;

    // ── Debug visual ─────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private GameObject _debugSquare;

    private void ShowDebugSquare()
    {
        if (_debugSquare != null) return;

        _debugSquare = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _debugSquare.name = "[DEBUG] PoisonIndicator";

        Destroy(_debugSquare.GetComponent<Collider>());
        Destroy(_debugSquare.GetComponent<MeshCollider>());

        _debugSquare.transform.SetParent(transform);
        _debugSquare.transform.localPosition = Vector3.zero;
        _debugSquare.transform.localScale    = new Vector3(2f, 2f, 1f);

        var renderer = _debugSquare.GetComponent<Renderer>();
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = new Color(0.2f, 0.8f, 0.1f, 0.55f); // verde semitransparente
        renderer.material = material;
    }

    private void HideDebugSquare()
    {
        if (_debugSquare != null)
            Destroy(_debugSquare);
    }
#endif

    // ── Configuración ────────────────────────────────────────────────────────

    /// <summary>
    /// Inyecta los parámetros base. Llamado por PoisonOnHitEffect antes de AddStack.
    /// </summary>
    public void Configure(int baseDamagePerTick, float tickInterval, int totalTicks, int maxStacks)
    {
        _baseDamagePerTick = baseDamagePerTick;
        _tickInterval      = tickInterval;
        _totalTicks        = totalTicks;
        _maxStacks         = maxStacks;
    }

    // ── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Añade un stack y reinicia el timer del DoT.
    /// Si ya se alcanzó el máximo, solo reinicia el timer.
    /// </summary>
    public void AddStack(Enemy enemy)
    {
        _currentStacks = Mathf.Min(_currentStacks + 1, _maxStacks);

        Debug.Log($"[Veneno] STACK aplicado en '{enemy.name}' → stacks actuales: {_currentStacks}/{_maxStacks}");

        if (_poisonCoroutine != null)
            StopCoroutine(_poisonCoroutine);

#if UNITY_EDITOR
        ShowDebugSquare();
#endif
        _poisonCoroutine = StartCoroutine(PoisonRoutine(enemy));
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private IEnumerator PoisonRoutine(Enemy enemy)
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

            int tickDamage = _baseDamagePerTick * _currentStacks;
            Debug.Log($"[Veneno] TICK en '{enemy.name}' → {tickDamage} daño (base {_baseDamagePerTick} × {_currentStacks} stacks) | vida restante: {enemy.currentLife - tickDamage}");
            enemy.TakeDamage(tickDamage);
        }

#if UNITY_EDITOR
        HideDebugSquare();
#endif
        _currentStacks = 0;
        Destroy(this);
    }
}