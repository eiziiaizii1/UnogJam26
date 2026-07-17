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
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashSeconds = 0.08f;

        private HealthComponent _health;
        private Color _baseColor;
        private Coroutine _routine;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _baseColor = _renderer.color;
        }

        private void OnEnable() => _health.Changed += OnHealthChanged;

        private void OnDisable() => _health.Changed -= OnHealthChanged;

        private void OnHealthChanged(int current, int max)
        {
            if (_renderer == null) return;
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
