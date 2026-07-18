using UnityEngine;
using Game.Runtime.Combat;

namespace Game.Runtime.Audio
{
    /// <summary>
    /// Self-wiring component that listens to a <see cref="HealthComponent"/>'s events
    /// and triggers hit/death sound effects on the SfxPlayer.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class HealthAudio : MonoBehaviour
    {
        [SerializeField] private SfxDefinition _hitSfx;
        [SerializeField] private SfxDefinition _deathSfx;

        private HealthComponent _health;
        private int _lastHealth = -1;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _health.Changed += OnHealthChanged;
            _health.Died += OnHealthDied;
            _lastHealth = _health.Current; // seed from real HP so the FIRST hit still plays
        }

        private void OnDisable()
        {
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnHealthDied;
        }

        private void OnHealthChanged(int current, int max)
        {
            // Only play hit SFX if health decreased (we took damage, not healed)
            if (current < _lastHealth)
            {
                if (SfxPlayer.Instance != null)
                {
                    SfxPlayer.Instance.Play(_hitSfx);
                }
            }

            _lastHealth = current;
        }

        private void OnHealthDied()
        {
            if (SfxPlayer.Instance != null)
            {
                SfxPlayer.Instance.Play(_deathSfx);
            }
        }
    }
}
