using UnityEngine;

/// <summary>
/// Implementación de IOnHitEffect que provoca quemadura en el enemigo.
/// Se construye con los parámetros de daño y se pasa al Projectile.
/// Si el enemigo ya tiene un BurnEffect activo, lo reinicia en lugar de crear uno nuevo
/// (evita el apilamiento infinito de Coroutines).
/// </summary>
public class BurnOnHitEffect : IOnHitEffect
{
    private readonly int   _damagePerTick;
    private readonly float _tickInterval;
    private readonly int   _totalTicks;

    public BurnOnHitEffect(int damagePerTick, float tickInterval, int totalTicks)
    {
        _damagePerTick = damagePerTick;
        _tickInterval  = tickInterval;
        _totalTicks    = totalTicks;
    }

    /// <summary>
    /// Busca o crea un BurnEffect en el enemigo y lo activa.
    /// </summary>
    public void Apply(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;

        // Reutilizamos el componente si ya existe (evita stacking)
        BurnEffect burn = enemy.GetComponent<BurnEffect>();
        if (burn == null)
        {
            burn = enemy.gameObject.AddComponent<BurnEffect>();
        }

        burn.Configure(_damagePerTick, _tickInterval, _totalTicks);
        burn.Apply(enemy);
    }
}
