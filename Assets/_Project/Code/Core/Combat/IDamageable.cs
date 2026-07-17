namespace Game.Core.Combat
{
    /// <summary>
    /// The narrow contract a bullet (or any damage source) needs (Guide §3.1 interface
    /// segregation). A shooter targeting <see cref="IDamageable"/> does not care whether the
    /// target is a crate, an enemy, or the player.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>True while the target can still take damage.</summary>
        bool IsAlive { get; }

        /// <summary>Applies <paramref name="amount"/> points of damage (must be non-negative).</summary>
        void ApplyDamage(int amount);
    }
}
