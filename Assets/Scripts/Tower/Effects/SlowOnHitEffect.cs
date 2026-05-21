using UnityEngine;

/// <summary>
/// Implementación de IOnHitEffect que ralentiza temporalmente al enemigo al impactar.
/// Si el enemigo ya tiene un SlowEffect activo, refresca la duración en lugar de apilar.
/// </summary>
public class SlowOnHitEffect : IOnHitEffect
{
    /// <summary>Probabilidad (0-1) de que el impacto aplique el slow.</summary>
    private readonly float _chance;

    /// <summary>Multiplicador de velocidad mientras dure el efecto (ej. 0.5 = 50% de velocidad).</summary>
    private readonly float _speedMultiplier;

    /// <summary>Segundos que dura el efecto antes de que el enemigo recupere su velocidad normal.</summary>
    private readonly float _duration;

    public SlowOnHitEffect(float chance, float speedMultiplier, float duration)
    {
        _chance          = chance;
        _speedMultiplier = speedMultiplier;
        _duration        = duration;
    }

    public void Apply(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;
        if (Random.value > _chance) return;

        SlowEffect slow = enemy.GetComponent<SlowEffect>();
        if (slow == null)
        {
            slow = enemy.gameObject.AddComponent<SlowEffect>();
        }

        slow.ApplySlow(_speedMultiplier, _duration);
    }
}
