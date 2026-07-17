using System;
using Game.Core.Combat;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Thin MonoBehaviour adapter over the pure <see cref="Health"/> model. Implements
    /// <see cref="IDamageable"/> so bullets can damage it, and re-broadcasts the model's events
    /// for views (HUD, death reactions) to observe.
    /// </summary>
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private int _maxHealth = 3;

        private Health _health;

        /// <summary>Re-broadcast of <see cref="Health.Changed"/> as (current, max).</summary>
        public event Action<int, int> Changed;

        /// <summary>Re-broadcast of <see cref="Health.Died"/>.</summary>
        public event Action Died;

        public bool IsAlive => _health?.IsAlive ?? true;

        private void Awake()
        {
            _health = new Health(_maxHealth);
            _health.Changed += (current, max) => Changed?.Invoke(current, max);
            _health.Died += () => Died?.Invoke();
        }

        public void ApplyDamage(int amount)
        {
            _health.TakeDamage(amount);
        }
    }
}
