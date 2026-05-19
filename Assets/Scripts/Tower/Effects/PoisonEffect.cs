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
    private int _baseDamagePerTick;
    private float _tickInterval;
    private int _totalTicks;
    private int _maxStacks;

    // ── Estado interno ───────────────────────────────────────────────────────
    private int _currentStacks;
    private Coroutine _poisonCoroutine;
    private GameObject _virusInstance;

    // ── Configuración ────────────────────────────────────────────────────────

    /// <summary>
    /// Inyecta los parámetros base. Llamado por PoisonOnHitEffect antes de AddStack.
    /// </summary>
    public void Configure(int baseDamagePerTick, float tickInterval, int totalTicks, int maxStacks)
    {
        _baseDamagePerTick = baseDamagePerTick;
        _tickInterval = tickInterval;
        _totalTicks = totalTicks;
        _maxStacks = maxStacks;
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

        if (_virusInstance == null)
        {
            GameObject virusPrefab = Resources.Load<GameObject>("VirusEffect");
            if (virusPrefab != null)
            {
                _virusInstance = Instantiate(virusPrefab, transform);
                _virusInstance.transform.localPosition = new Vector3(0, 0, -1f);
            }
            else
            {
                Debug.LogError("NO ENCUENTRA EL PREFAB VirusEffect en Resources/");
            }
        }

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
                DestroyVirus();
                yield break;
            }

            int tickDamage = _baseDamagePerTick * _currentStacks;
            Debug.Log($"[Veneno] TICK en '{enemy.name}' → {tickDamage} daño (base {_baseDamagePerTick} × {_currentStacks} stacks) | vida restante: {enemy.currentLife - tickDamage}");
            enemy.TakeDamage(tickDamage);
        }

        _currentStacks = 0;
        DestroyVirus();
        Destroy(this);
    }

    private void DestroyVirus()
    {
        if (_virusInstance != null)
        {
            Destroy(_virusInstance);
            _virusInstance = null;
        }
    }
}