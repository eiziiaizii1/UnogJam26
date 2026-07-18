using PrimeTween;
using TMPro;
using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// A small world-space speech bubble that pops out of whatever it is parented to. Presentation
    /// only — it owns no input and no notion of who opened it, so the same prefab works for a
    /// carcass, an NPC or a sign.
    /// <para>
    /// The pop-in and typewriter follow the pattern already used by the story-scene
    /// <c>DialogueController</c>: PrimeTween scale + alpha, and a custom tween driving
    /// <see cref="TMP_Text.maxVisibleCharacters"/> so the reveal costs no per-character allocation.
    /// </para>
    /// </summary>
    public sealed class DialogueBubble : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _text;

        [Header("Tuning")]
        [SerializeField] private float _popSeconds = 0.22f;
        [Tooltip("Seconds per character.")]
        [SerializeField] private float _typeSpeed = 0.03f;

        private string[] _lines;
        private int _index;
        private Tween _typing;

        // The authored scale, captured before anything animates it. A world-space canvas is tiny
        // (~0.016) so popping to Vector3.one would inflate the bubble to ~60x its size and swallow
        // the screen — the scale must return to what the prefab authored, not to 1.
        private Vector3 _openScale = Vector3.one;

        public bool IsOpen { get; private set; }

        /// <summary>True while characters are still being revealed.</summary>
        public bool IsTyping => _typing.isAlive;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_text == null) _text = GetComponentInChildren<TMP_Text>();

            var scale = _canvasGroup.transform.localScale;
            if (scale != Vector3.zero) _openScale = scale;

            ApplyClosedState();
        }

        private void ApplyClosedState()
        {
            IsOpen = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.transform.localScale = Vector3.zero;
            }
            if (_text != null) _text.maxVisibleCharacters = 0;
        }

        public void Show(string[] lines)
        {
            if (lines == null || lines.Length == 0) return;

            _lines = lines;
            _index = 0;
            IsOpen = true;

            StopTweens();
            _canvasGroup.transform.localScale = Vector3.zero;
            Tween.Alpha(_canvasGroup, 1f, _popSeconds);
            Tween.Scale(_canvasGroup.transform, _openScale, _popSeconds, ease: Ease.OutBack);

            // Start typing immediately rather than on pop-complete: the bubble reads as "already
            // talking" as it grows, which feels quicker than waiting out the scale tween.
            TypeCurrentLine();
        }

        /// <summary>
        /// Completes the current line if still typing, otherwise advances. Returns false when the
        /// last line has been dismissed and the bubble has closed.
        /// </summary>
        public bool Advance()
        {
            if (!IsOpen) return false;

            if (IsTyping)
            {
                _typing.Stop();
                _text.maxVisibleCharacters = _text.text.Length;
                return true;
            }

            _index++;
            if (_index >= _lines.Length)
            {
                Close();
                return false;
            }

            TypeCurrentLine();
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            StopTweens();
            Tween.Alpha(_canvasGroup, 0f, _popSeconds);
            Tween.Scale(_canvasGroup.transform, Vector3.zero, _popSeconds, ease: Ease.InBack);
        }

        private void TypeCurrentLine()
        {
            string line = _lines[_index];
            _text.text = line;
            _text.maxVisibleCharacters = 0;

            _typing.Stop();
            _typing = Tween.Custom(0f, line.Length, line.Length * _typeSpeed,
                onValueChange: v => _text.maxVisibleCharacters = Mathf.RoundToInt(v),
                ease: Ease.Linear);
        }

        private void StopTweens()
        {
            _typing.Stop();
            Tween.StopAll(_canvasGroup);
            Tween.StopAll(_canvasGroup.transform);
        }

        private void OnDisable()
        {
            StopTweens();
            ApplyClosedState();
        }
    }
}
