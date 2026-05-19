using System.Collections;
using UnityEngine;

/// <summary>
/// Efecto de quemadura (DoT de fuego). Instancia el prefab "FireEffect"
/// encima del enemigo mientras dura el efecto y lo destruye al terminar.
/// </summary>
public class BurnEffect : MonoBehaviour, IOnHitEffect
{
    private int _damagePerTick;
    private float _tickInterval;
    private int _totalTicks;

    private Coroutine _burnCoroutine;
    private GameObject _fireInstance;

    // ── IOnHitEffect ────────────────────────────────────────────────────────

    public void Apply(Enemy enemy)
    {
        if (_burnCoroutine != null)
            StopCoroutine(_burnCoroutine);

        if (_fireInstance == null)
        {
            GameObject firePrefab = Resources.Load<GameObject>("FireEffect");
            if (firePrefab != null)
            {
                _fireInstance = Instantiate(firePrefab, transform);
                _fireInstance.transform.localPosition = new Vector3(0, 0, -1f);
            }
            else
            {
                Debug.LogError("NO ENCUENTRA EL PREFAB FireEffect en Resources/");
            }
        }

        _burnCoroutine = StartCoroutine(BurnRoutine(enemy));
    }

    public void Configure(int damagePerTick, float tickInterval, int totalTicks)
    {
        _damagePerTick = damagePerTick;
        _tickInterval = tickInterval;
        _totalTicks = totalTicks;
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private IEnumerator BurnRoutine(Enemy enemy)
    {
        for (int i = 0; i < _totalTicks; i++)
        {
            yield return new WaitForSeconds(_tickInterval);

            if (enemy == null || enemy.IsDead)
            {
                DestroyFire();
                yield break;
            }

            enemy.TakeDamage(_damagePerTick);
        }

        DestroyFire();
        Destroy(this);
    }

    private void DestroyFire()
    {
        if (_fireInstance != null)
        {
            Destroy(_fireInstance);
            _fireInstance = null;
        }
    }
}