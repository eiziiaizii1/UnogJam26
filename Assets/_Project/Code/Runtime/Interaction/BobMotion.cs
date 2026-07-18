using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Sine bob around the local start position. Used by the "!" prompt so it reads as alive
    /// rather than a decal stuck above the carcass. Deliberately transform-only and stateless
    /// enough to survive being toggled on and off with its GameObject.
    /// </summary>
    public sealed class BobMotion : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 0.12f;
        [SerializeField] private float _frequency = 1.6f;
        [Tooltip("Pops from zero to full scale on enable, so the marker appears with some snap.")]
        [SerializeField] private float _popSeconds = 0.15f;

        private Vector3 _basePosition;
        private Vector3 _baseScale;
        private float _popTimer;

        private void Awake()
        {
            _basePosition = transform.localPosition;
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            _popTimer = 0f;
            transform.localPosition = _basePosition;
            transform.localScale = _popSeconds > 0f ? Vector3.zero : _baseScale;
        }

        private void Update()
        {
            if (_popTimer < _popSeconds)
            {
                _popTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_popTimer / _popSeconds);
                // Slight overshoot so it lands with a pop instead of easing in flat.
                float overshoot = 1f + 0.25f * Mathf.Sin(t * Mathf.PI);
                transform.localScale = _baseScale * (t * overshoot);
            }

            float offset = Mathf.Sin(Time.time * _frequency * Mathf.PI * 2f) * _amplitude;
            transform.localPosition = _basePosition + new Vector3(0f, offset, 0f);
        }
    }
}
