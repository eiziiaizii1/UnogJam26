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
            // Skip if a spawner already configured us via SetMaxHealth (data-driven enemies).
            if (_health == null)
            {
                Build(_maxHealth);
            }
        }

        /// <summary>Overrides max HP from data (e.g. an EnemyDefinition) and resets to full.</summary>
        public void SetMaxHealth(int maxHealth)
        {
            Build(maxHealth);
        }

        public void ApplyDamage(int amount)
        {
            _health.TakeDamage(amount);
        }

        private void Build(int maxHealth)
        {
            _maxHealth = maxHealth;
            _health = new Health(maxHealth);
            _health.Changed += OnHealthChanged;
            _health.Died += OnHealthDied;
        }

        private void OnHealthChanged(int current, int max) => Changed?.Invoke(current, max);

        private void OnHealthDied() => Died?.Invoke();
    }
}
