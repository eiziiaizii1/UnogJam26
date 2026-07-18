using Game.Runtime.Combat;
using UnityEngine;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Binds a <see cref="HealthComponent"/> to the <see cref="AnimatedHealthBar"/> view
    /// (Guide §4.2 MVP — the bar stays a humble view that renders a number; it never reads game
    /// state itself, and the health system never knows a UI exists).
    /// <para>
    /// Put this on the health bar object. If <c>_health</c> is left empty it auto-finds the
    /// GameObject tagged "Player", so it keeps working if the player is re-created.
    /// </para>
    /// </summary>
    public sealed class HealthBarPresenter : MonoBehaviour
    {
        [SerializeField] private AnimatedHealthBar _bar;
        [Tooltip("Leave empty to auto-find the GameObject tagged 'Player'.")]
        [SerializeField] private HealthComponent _health;

        private void Awake()
        {
            if (_bar == null) _bar = GetComponent<AnimatedHealthBar>();

            if (_health == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) _health = player.GetComponent<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            if (_health != null) _health.Changed += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Changed -= OnHealthChanged;
        }

        // Start (after every Awake) so the real range wins over the bar's serialized defaults.
        private void Start()
        {
            if (_health == null)
            {
                Debug.LogWarning($"[{nameof(HealthBarPresenter)}] No HealthComponent found — the bar will not react to damage.", this);
                return;
            }

            if (_bar != null) _bar.Initialize(_health.Current, _health.Max);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_bar != null) _bar.SetHealth(current);
        }
    }
}
