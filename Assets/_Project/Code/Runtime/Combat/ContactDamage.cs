using Game.Core.Combat;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Deals damage to whatever <see cref="IDamageable"/> it stays in contact with, on a cooldown
    /// (Guide §5.4 — damage flows through the same contract bullets use). Reusable for enemies,
    /// hazards, spikes. The enemy's damage value is pushed in from its <c>EnemyDefinition</c>.
    /// </summary>
    public sealed class ContactDamage : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _damageIntervalSeconds = 0.75f;

        private float _nextDamageTime;

        /// <summary>Sets the per-hit damage (called by the enemy from its archetype).</summary>
        public void SetDamage(int damage) => _damage = damage;

        private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);

        private void OnCollisionStay2D(Collision2D collision) => TryDamage(collision.collider);

        private void TryDamage(Collider2D other)
        {
            if (_damage <= 0 || Time.time < _nextDamageTime) return;
            if (other.gameObject == gameObject) return;

            if (other.TryGetComponent<IDamageable>(out var damageable) && damageable.IsAlive)
            {
                damageable.ApplyDamage(_damage);
                _nextDamageTime = Time.time + _damageIntervalSeconds;
            }
        }
    }
}
