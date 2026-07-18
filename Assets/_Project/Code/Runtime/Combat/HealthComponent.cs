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

        /// <summary>Current hit points (for listeners/HUD to seed or display state).</summary>
        public int Current => _health?.Current ?? _maxHealth;

        /// <summary>Maximum hit points.</summary>
        public int Max => _health?.Max ?? _maxHealth;

        private void Awake()
        {
            // Skip if a spawner already configured us via SetMaxHealth (data-driven enemies).
            if (_health == null)
            {
                Build(_maxHealth, notify: false);
            }
        }

        /// <summary>Overrides max HP from data (e.g. an EnemyDefinition) and resets to full.</summary>
        public void SetMaxHealth(int maxHealth)
        {
            Build(maxHealth, notify: true);
        }

        /// <summary>Restores full HP (used on player respawn). Notifies so views resync.</summary>
        public void ResetToFull()
        {
            Build(_maxHealth, notify: true);
        }

        /// <summary>Raises max HP (upgrades) and restores to full. Notifies so the health bar rescales.</summary>
        public void AddMaxHealth(int bonus)
        {
            if (bonus <= 0) return;
            Build(_maxHealth + bonus, notify: true);
        }

        public void ApplyDamage(int amount)
        {
            _health.TakeDamage(amount);
        }

        private void Build(int maxHealth, bool notify)
        {
            _maxHealth = maxHealth;
            _health = new Health(maxHealth);
            _health.Changed += OnHealthChanged;
            _health.Died += OnHealthDied;

            // Announce the reset so views (health bar) resync — e.g. after a respawn.
            if (notify) Changed?.Invoke(_health.Current, _health.Max);
        }

        private void OnHealthChanged(int current, int max) => Changed?.Invoke(current, max);

        private void OnHealthDied() => Died?.Invoke();
    }
}
