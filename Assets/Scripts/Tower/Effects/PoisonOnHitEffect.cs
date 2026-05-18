using UnityEngine;

/// <summary>
/// Implementación de IOnHitEffect que aplica veneno apilable al enemigo.
/// Cada impacto añade un stack: si el enemigo ya tiene PoisonEffect,
/// incrementa los stacks y reinicia el timer en lugar de crear una instancia nueva.
/// </summary>
public class PoisonOnHitEffect : IOnHitEffect
{
    private readonly int   _baseDamagePerTick;
    private readonly float _tickInterval;
    private readonly int   _totalTicks;
    private readonly int   _maxStacks;

    public PoisonOnHitEffect(int baseDamagePerTick, float tickInterval, int totalTicks, int maxStacks)
    {
        _baseDamagePerTick = baseDamagePerTick;
        _tickInterval      = tickInterval;
        _totalTicks        = totalTicks;
        _maxStacks         = maxStacks;
    }

    /// <summary>
    /// Busca o crea un PoisonEffect en el enemigo y añade un stack.
    /// </summary>
    public void Apply(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;

        PoisonEffect poison = enemy.GetComponent<PoisonEffect>();
        if (poison == null)
        {
            poison = enemy.gameObject.AddComponent<PoisonEffect>();
            poison.Configure(_baseDamagePerTick, _tickInterval, _totalTicks, _maxStacks);
        }

        poison.AddStack(enemy);
    }
}
