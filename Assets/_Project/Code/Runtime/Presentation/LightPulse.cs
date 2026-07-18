using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// Drives a <see cref="Light2D"/> through a short attack/decay envelope — the difference
    /// between a light that blinks and one that *flashes*. Reused by the muzzle flash, bullet
    /// trails and impact bursts, so every light in the game falls off the same way.
    /// <para>
    /// It plays on enable by default, so a caller that only toggles the GameObject (the muzzle
    /// flash in <see cref="Game.Runtime.Combat.Shooter"/> does exactly that) gets the envelope
    /// for free without knowing this component exists.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public sealed class LightPulse : MonoBehaviour
    {
        [Header("Envelope")]
        [SerializeField] private float _peakIntensity = 3f;
        [Tooltip("Rise time to peak. Keep tiny — a flash should arrive instantly.")]
        [SerializeField] private float _attackSeconds = 0.02f;
        [SerializeField] private float _decaySeconds = 0.12f;

        [Header("Optional radius punch (point lights only)")]
        [Tooltip("Outer radius added at peak, on top of the authored radius. 0 = radius untouched.")]
        [SerializeField] private float _radiusPunch;

        [Header("Behaviour")]
        [Tooltip("Unscaled by default so a hit-stop doesn't stretch the flash into slow motion.")]
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _playOnEnable = true;

        private Light2D _light;
        private float _baseRadius;
        private bool _canPunchRadius;
        private float _elapsed;
        private bool _playing;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _baseRadius = _light.pointLightOuterRadius;
            _canPunchRadius = _radiusPunch > 0f && _light.lightType == Light2D.LightType.Point;
        }

        private void OnEnable()
        {
            if (_playOnEnable) Play();
            else Apply(0f);
        }

        /// <summary>Restarts the envelope from zero. Safe to call mid-pulse (retrigger).</summary>
        public void Play()
        {
            _elapsed = 0f;
            _playing = true;
            Apply(0f);
        }

        private void Update()
        {
            if (!_playing) return;

            _elapsed += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_elapsed >= _attackSeconds + _decaySeconds)
            {
                _playing = false;
                Apply(0f);
                return;
            }

            Apply(Envelope(_elapsed));
        }

        /// <summary>Normalised 0..1..0 shape: linear rise over attack, linear fall over decay.</summary>
        private float Envelope(float elapsed)
        {
            if (elapsed <= _attackSeconds)
            {
                return _attackSeconds <= 0f ? 1f : elapsed / _attackSeconds;
            }

            float decayed = (elapsed - _attackSeconds) / Mathf.Max(_decaySeconds, 0.0001f);
            return 1f - decayed;
        }

        private void Apply(float normalized)
        {
            _light.intensity = _peakIntensity * normalized;

            if (_canPunchRadius)
            {
                _light.pointLightOuterRadius = _baseRadius + _radiusPunch * normalized;
            }
        }
    }
}
