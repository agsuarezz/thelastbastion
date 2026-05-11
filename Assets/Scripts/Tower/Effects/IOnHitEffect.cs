/// <summary>
/// Contrato que deben cumplir todos los efectos al impacto que puede
/// llevar un proyectil. Permite añadir nuevos efectos (veneno, hielo…)
/// sin tocar Projectile ni Enemy (Principio Abierto/Cerrado).
/// </summary>
public interface IOnHitEffect
{
    /// <summary>
    /// Aplica el efecto sobre el enemigo objetivo.
    /// </summary>
    /// <param name="enemy">El enemigo que acaba de recibir el impacto.</param>
    void Apply(Enemy enemy);
}
