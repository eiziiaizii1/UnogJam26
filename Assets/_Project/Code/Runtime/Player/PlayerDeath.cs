using System;
using Game.Runtime.Combat;
using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Player-death reaction. For now a **stub** (Guide §13.4): the player has nowhere to respawn
    /// until the level-end trigger and game flow are wired (slice 5), so death disables control,
    /// dims the sprite, and raises <see cref="Died"/> for a future GameFlow listener to restart on.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class PlayerDeath : MonoBehaviour
    {
        [Tooltip("Components switched off on death (motor, shooter, input).")]
        [SerializeField] private Behaviour[] _disableOnDeath;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _deadColor = new(0.2f, 0.2f, 0.2f);

        private HealthComponent _health;

        /// <summary>Raised once when the player dies (for GameFlow to observe later).</summary>
        public event Action Died;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable() => _health.Died += HandleDied;

        private void OnDisable() => _health.Died -= HandleDied;

        private void HandleDied()
        {
            Debug.Log("[Player] died — restart wiring arrives with the level-end trigger (slice 5).");

            if (_disableOnDeath != null)
            {
                foreach (var behaviour in _disableOnDeath)
                {
                    if (behaviour != null) behaviour.enabled = false;
                }
            }

            if (_renderer != null) _renderer.color = _deadColor;
            Died?.Invoke();
        }
    }
}
