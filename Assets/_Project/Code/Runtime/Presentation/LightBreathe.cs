using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// Continuous sine "breathing" on a <see cref="Light2D"/>'s intensity — the steady-state
    /// counterpart to <see cref="LightPulse"/>, which is a one-shot attack/decay envelope and the
    /// wrong shape for something that should glow forever.
    /// <para>
    /// Each instance starts at a random phase by default, otherwise a field of collectibles pulses
    /// in perfect lockstep and reads as a machine rather than as scattered loot.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public sealed class LightBreathe : MonoBehaviour
    {
        [SerializeField] private float _minIntensity = 0.8f;
        [SerializeField] private float _maxIntensity = 1.8f;
        [Tooltip("Full bright-dim-bright cycles per second.")]
        [SerializeField] private float _cyclesPerSecond = 0.5f;
        [SerializeField] private bool _randomisePhase = true;

        private Light2D _light;
        private float _phase;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _phase = _randomisePhase ? Random.value * Mathf.PI * 2f : 0f;
        }

        private void Update()
        {
            // 0..1 sine, so the light never inverts past the authored min/max.
            float t = (Mathf.Sin(Time.time * _cyclesPerSecond * Mathf.PI * 2f + _phase) + 1f) * 0.5f;
            _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);
        }
    }
}
