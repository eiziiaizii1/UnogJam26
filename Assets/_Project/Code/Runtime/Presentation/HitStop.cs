using UnityEngine;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// Freeze frames: dips <see cref="Time.timeScale"/> for a few dozen milliseconds so a hit
    /// lands with weight. The cheapest gamefeel win there is.
    /// <para>
    /// Self-installing (same reasoning as <see cref="Game.Runtime.Boot.GameBootstrap"/>): call
    /// sites just use <see cref="Play"/> and no scene needs wiring. Drop one in a scene only if
    /// you want to tune the frozen scale in the Inspector.
    /// </para>
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        [Tooltip("Time scale held during the stop. 0 is a hard freeze; a hair above reads softer.")]
        [SerializeField] private float _frozenTimeScale = 0.05f;
        [SerializeField] private float _defaultSeconds = 0.045f;

        private static HitStop _instance;

        private float _remainingRealSeconds;
        private float _restoreTimeScale = 1f;
        private bool _active;

        /// <summary>
        /// Freezes for <paramref name="seconds"/> of real time (non-positive = the tuned default).
        /// Overlapping calls extend the stop rather than stacking or clobbering the restore value.
        /// </summary>
        public static void Play(float seconds = 0f)
        {
            if (!Application.isPlaying) return;

            var instance = Resolve();
            if (instance != null) instance.Begin(seconds);
        }

        private static HitStop Resolve()
        {
            if (_instance != null) return _instance;

            _instance = FindAnyObjectByType<HitStop>();
            if (_instance != null) return _instance;

            var host = new GameObject(nameof(HitStop));
            _instance = host.AddComponent<HitStop>();
            return _instance;
        }

        private void Awake()
        {
            // An authored instance wins; a duplicate from an additive load bows out.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Begin(float seconds)
        {
            float duration = seconds > 0f ? seconds : _defaultSeconds;

            // Only capture the restore value on the *first* stop, or a retrigger mid-freeze
            // would memorise the frozen scale and never hand time back.
            if (!_active)
            {
                _restoreTimeScale = Time.timeScale;
                _active = true;
            }

            _remainingRealSeconds = Mathf.Max(_remainingRealSeconds, duration);
            Time.timeScale = _frozenTimeScale;
        }

        private void Update()
        {
            if (!_active) return;

            _remainingRealSeconds -= Time.unscaledDeltaTime;
            if (_remainingRealSeconds > 0f) return;

            Time.timeScale = _restoreTimeScale;
            _active = false;
        }

        private void OnDestroy()
        {
            // Never leave the game frozen because the driver went away mid-stop.
            if (_active) Time.timeScale = _restoreTimeScale;
            if (_instance == this) _instance = null;
        }
    }
}
