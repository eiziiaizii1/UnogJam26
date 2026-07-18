using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Premium animated health bar.
    /// Features dual-slider delay drain (fighting game/Souls-like effect), color grading, 
    /// punch scale feedback on impact, and smooth recovery transitions.
    /// </summary>
    public sealed class AnimatedHealthBar : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider _mainSlider;
        [Tooltip("The background slider that catches up slowly after damage (typically yellow or white).")]
        [SerializeField] private Slider _delayedSlider;

        [Header("Color Grading")]
        [Tooltip("The UI Image component of the main slider's fill to color-grade dynamically.")]
        [SerializeField] private Image _mainFillImage;
        [SerializeField] private Gradient _healthColorGradient;

        [Header("Visual Impact Feel")]
        [Tooltip("Object to punch/shake on damage. If left empty, falls back to the main slider.")]
        [SerializeField] private Transform _punchTarget;
        [SerializeField] private Vector3 _damagePunchStrength = new Vector3(0.08f, -0.08f, 0f);
        [SerializeField] private Vector3 _healPunchStrength = new Vector3(0.05f, 0.05f, 0f);
        [SerializeField] private float _punchDuration = 0.15f;

        [Header("Animation Speeds")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;
        [SerializeField] private float _mainAnimationDuration = 0.12f;
        [SerializeField] private float _delayedAnimationDuration = 0.35f;
        [SerializeField] private float _delayedStartDelay = 0.3f;
        [SerializeField] private Ease _animationEase = Ease.OutQuad;

        [Header("Mock Test Options")]
        [Tooltip("Debug only: press K to damage the bar directly. Off by default — real damage drives it via HealthBarPresenter.")]
        [SerializeField] private bool _enableMockDamageKey;
        [SerializeField] private float _damageAmount = 15f;

        private Tween _mainTween;
        private Tween _delayedTween;
        private Tween _punchTween;
        private float _previousHealth;

        // Awake (not Start) so a presenter can override with real values in Start without a race.
        private void Awake()
        {
            Initialize(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Snaps the bar to a health range with no animation. Call this once from a presenter so
        /// the bar matches the real <c>HealthComponent</c> (e.g. 5 HP, not the mock 100).
        /// </summary>
        public void Initialize(float currentHealth, float maxHealth)
        {
            _maxHealth = Mathf.Max(1f, maxHealth);
            _currentHealth = Mathf.Clamp(currentHealth, 0f, _maxHealth);
            _previousHealth = _currentHealth;

            if (_mainSlider != null)
            {
                _mainSlider.minValue = 0f;
                _mainSlider.maxValue = _maxHealth;
                _mainSlider.value = _currentHealth;
            }

            if (_delayedSlider != null)
            {
                _delayedSlider.minValue = 0f;
                _delayedSlider.maxValue = _maxHealth;
                _delayedSlider.value = _currentHealth;
            }

            UpdateFillColor();
        }

        private void Update()
        {
            if (!_enableMockDamageKey) return;

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.K))
            {
                TakeDamage(_damageAmount);
            }
        }

        /// <summary>
        /// Reduces health and triggers the premium damage sequence.
        /// If health drops below zero, wraps back to max health for testing.
        /// </summary>
        public void TakeDamage(float amount)
        {
            float targetHealth = _currentHealth - amount;
            if (targetHealth <= 0f)
            {
                SetHealth(_maxHealth); // Wrap back (Heal)
            }
            else
            {
                SetHealth(targetHealth);
            }
        }

        /// <summary>
        /// Sets current health and manages different tween paths for damage vs healing.
        /// </summary>
        public void SetHealth(float targetHealth)
        {
            targetHealth = Mathf.Clamp(targetHealth, 0f, _maxHealth);
            if (Mathf.Approximately(_currentHealth, targetHealth)) return;

            float oldHealth = _currentHealth;
            _currentHealth = targetHealth;

            bool isDamage = _currentHealth < oldHealth;

            // 1. Animate Main Slider
            if (_mainSlider != null)
            {
                _mainTween.Stop();
                
                // On damage, the main bar drops immediately for fast feedback
                float duration = isDamage ? _mainAnimationDuration : _delayedAnimationDuration;
                _mainTween = Tween.Custom(_mainSlider, _mainSlider.value, _currentHealth, duration, 
                    (s, val) => {
                        s.value = val;
                        UpdateFillColor();
                    }, 
                    _animationEase);
            }

            // 2. Animate Delayed Slider
            if (_delayedSlider != null)
            {
                _delayedTween.Stop();

                if (isDamage)
                {
                    // Damage: Wait, then catch up slowly
                    _delayedTween = Tween.Custom(_delayedSlider, _delayedSlider.value, _currentHealth, 
                        _delayedAnimationDuration, (s, val) => s.value = val, _animationEase, startDelay: _delayedStartDelay);
                }
                else
                {
                    // Healing: Catch up instantly alongside the main bar
                    _delayedTween = Tween.Custom(_delayedSlider, _delayedSlider.value, _currentHealth, 
                        _delayedAnimationDuration, (s, val) => s.value = val, _animationEase);
                }
            }

            // 3. Impact juice (Punch scale)
            Transform punchObj = _punchTarget != null ? _punchTarget : (_mainSlider != null ? _mainSlider.transform : null);
            if (punchObj != null)
            {
                _punchTween.Stop();
                Vector3 strength = isDamage ? _damagePunchStrength : _healPunchStrength;
                _punchTween = Tween.PunchScale(punchObj, strength, _punchDuration);
            }

            _previousHealth = _currentHealth;
        }

        private void UpdateFillColor()
        {
            if (_mainFillImage == null || _healthColorGradient == null) return;
            
            float percent = (_mainSlider != null ? _mainSlider.value : _currentHealth) / _maxHealth;
            _mainFillImage.color = _healthColorGradient.Evaluate(percent);
        }
    }
}
