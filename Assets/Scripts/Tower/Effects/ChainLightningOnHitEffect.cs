/// <summary>
/// Implementación de IOnHitEffect que activa la cadena eléctrica al impactar.
/// Crea un ChainLightningEffect temporal en el enemigo y lo lanza.
/// No apila: si el enemigo ya tiene una cadena activa, la nueva la reemplaza.
/// </summary>
public class ChainLightningOnHitEffect : IOnHitEffect
{
    private readonly float _damage;
    private readonly float _radius;
    private readonly int   _maxJumps;
    private readonly float _damageFalloff;

    public ChainLightningOnHitEffect(float damage, float radius, int maxJumps, float damageFalloff)
    {
        _damage        = damage;
        _radius        = radius;
        _maxJumps      = maxJumps;
        _damageFalloff = damageFalloff;
    }

    public void Apply(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;

        // Reutilizamos componente si ya existe para evitar solapamiento
        ChainLightningEffect chain = enemy.GetComponent<ChainLightningEffect>();
        if (chain != null)
            UnityEngine.Object.Destroy(chain);

        chain = enemy.gameObject.AddComponent<ChainLightningEffect>();
        chain.Configure(_damage, _radius, _maxJumps, _damageFalloff);
        chain.Activate(enemy);
    }
}
