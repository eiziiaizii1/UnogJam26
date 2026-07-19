using PrimeTween;
using TMPro;
using UnityEngine;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// The "you just got stronger" beat at the start of a level: a label that rises and fades above
    /// the robot, plus a glow that swells and dies away. Presentation only — it is told when to play
    /// and never decides whether an upgrade happened.
    /// <para>
    /// Lives on the player prefab so it follows the robot without any anchoring logic, and starts
    /// hidden so a level the player enters with no upgrade shows nothing.
    /// </para>
    /// </summary>
    public sealed class LevelUpIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _label;
        [Tooltip("Child holding a Light2D + LightPulse. Toggled on for the duration of the flourish.")]
        [SerializeField] private GameObject _glow;

        [Header("Tuning")]
        [SerializeField] private float _fadeInSeconds = 0.25f;
        [SerializeField] private float _holdSeconds = 1.6f;
        [SerializeField] private float _fadeOutSeconds = 0.6f;
        [Tooltip("How far the label drifts upward over its lifetime, in world units.")]
        [SerializeField] private float _riseDistance = 0.7f;

        private Vector3 _labelHomePosition;
        private Sequence _sequence;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            if (_label == null) _label = GetComponentInChildren<TMP_Text>(true);

            if (_canvasGroup != null) _labelHomePosition = _canvasGroup.transform.localPosition;
            ApplyHiddenState();
        }

        private void ApplyHiddenState()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.transform.localPosition = _labelHomePosition;
            }
            if (_glow != null) _glow.SetActive(false);
        }

        /// <summary>Plays the flourish. Safe to call again mid-play — it restarts cleanly.</summary>
        public void Play(string message = null)
        {
            if (_canvasGroup == null) return;

            if (!string.IsNullOrEmpty(message) && _label != null) _label.text = message;

            if (_sequence.isAlive) _sequence.Stop();
            Tween.StopAll(_canvasGroup);
            Tween.StopAll(_canvasGroup.transform);

            _canvasGroup.alpha = 0f;
            _canvasGroup.transform.localPosition = _labelHomePosition;

            // Re-enabling the glow retriggers its LightPulse envelope via OnEnable.
            if (_glow != null)
            {
                _glow.SetActive(false);
                _glow.SetActive(true);
            }

            float total = _fadeInSeconds + _holdSeconds + _fadeOutSeconds;

            // The rise runs across the whole flourish while the alpha is sequenced separately,
            // so the label keeps drifting while it fades instead of stopping dead.
            Tween.LocalPositionY(_canvasGroup.transform,
                _labelHomePosition.y + _riseDistance, total, ease: Ease.OutCubic);

            _sequence = Sequence.Create()
                .Chain(Tween.Alpha(_canvasGroup, 1f, _fadeInSeconds, ease: Ease.OutQuad))
                .ChainDelay(_holdSeconds)
                .Chain(Tween.Alpha(_canvasGroup, 0f, _fadeOutSeconds, ease: Ease.InQuad))
                .ChainCallback(() => { if (_glow != null) _glow.SetActive(false); });
        }

        private void OnDisable()
        {
            if (_sequence.isAlive) _sequence.Stop();
            ApplyHiddenState();
        }
    }
}
