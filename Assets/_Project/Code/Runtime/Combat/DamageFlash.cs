using System.Collections;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Briefly flashes the sprite when its <see cref="HealthComponent"/> changes — cheap hit
    /// feedback until the real HUD exists. Reused by player, enemies, and crates. (A PrimeTween
    /// version is a fine M4-polish swap; a coroutine keeps this dependency-free for now.)
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class DamageFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Tint multiplied over the sprite. Must NOT be white — SpriteRenderer.color multiplies, " +
                 "so white leaves white-tinted art untouched and the flash is invisible.")]
        [SerializeField] private Color _flashColor = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float _flashSeconds = 0.08f;

        private HealthComponent _health;
        private Color _baseColor;
        private Coroutine _routine;
        private int _lastKnownHealth;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            // Search children too: the sprite usually lives on a "Visuals" child, and a same-object-only
            // lookup silently leaves this null — the flash then does nothing, with no error to notice.
            if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
            if (_renderer != null) _baseColor = _renderer.color;
        }

        private void OnEnable()
        {
            _health.Changed += OnHealthChanged;
            _lastKnownHealth = _health.Current;
        }

        private void OnDisable() => _health.Changed -= OnHealthChanged;

        private void OnHealthChanged(int current, int max)
        {
            // Only damage flashes. Heals, respawn resets and max-HP upgrades all raise Changed too.
            bool tookDamage = current < _lastKnownHealth;
            _lastKnownHealth = current;

            if (!tookDamage || _renderer == null) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            _renderer.color = _flashColor;
            yield return new WaitForSeconds(_flashSeconds);
            // Don't restore if this was the killing blow — a death handler may have recolored us.
            if (_health.IsAlive) _renderer.color = _baseColor;
        }
    }
}
