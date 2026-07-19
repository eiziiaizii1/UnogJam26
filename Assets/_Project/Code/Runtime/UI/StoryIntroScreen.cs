using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PrimeTween;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Manages the cinematic story slideshow sequence before Level 1.
    /// Supports typewriter text effect, smooth crossfades, and skipping.
    /// </summary>
    public sealed class StoryIntroScreen : MonoBehaviour
    {
        [System.Serializable]
        public sealed class StoryCard
        {
            [Tooltip("The artwork/illustration shown at the top.")]
            public Sprite Image;
            
            [TextArea(3, 10)]
            [Tooltip("The story text narration shown below the image.")]
            public string NarrationText;
        }

        [Header("References")]
        [SerializeField] private Image _uiImage;
        [SerializeField] private TextMeshProUGUI _uiText;
        [SerializeField] private CanvasGroup _cardCanvasGroup;
        [SerializeField] private GameObject _nextIndicator;

        [Header("Content")]
        [SerializeField] private StoryCard[] _cards;

        [Header("Settings")]
        [SerializeField] private float _typewriterSpeed = 0.04f;
        [SerializeField] private float _cardFadeDuration = 0.4f;
        [SerializeField] private string _nextSceneName = "Level_01";

        private int _currentCardIndex = -1;
        private Coroutine _typewriterCoroutine;
        private bool _isTextFullyTyped;
        private string _currentFullText = "";
        private bool _isTransitioning;

        private void Start()
        {
            // Reset TimeScale
            Time.timeScale = 1f;

            if (_cardCanvasGroup != null)
            {
                _cardCanvasGroup.alpha = 0f;
            }

            if (_nextIndicator != null)
            {
                _nextIndicator.SetActive(false);
            }

            if (_cards == null || _cards.Length == 0)
            {
                Debug.LogWarning("[StoryIntro] No story cards configured! Transitioning directly to next scene.");
                GoToNextScene();
                return;
            }

            ShowNextCard();
        }

        private void Update()
        {
            if (_isTransitioning) return;

            // Advance / Skip input: Space, Return/Enter, or left mouse click
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) || 
                UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return) || 
                UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (!_isTextFullyTyped)
                {
                    // Instantly complete typing
                    CompleteTypewriter();
                }
                else
                {
                    // Advance to next card
                    ShowNextCard();
                }
            }
        }

        private void ShowNextCard()
        {
            _currentCardIndex++;

            if (_currentCardIndex >= _cards.Length)
            {
                GoToNextScene();
                return;
            }

            StartCoroutine(ShowCardRoutine(_cards[_currentCardIndex]));
        }

        private IEnumerator ShowCardRoutine(StoryCard card)
        {
            _isTransitioning = true;
            if (_nextIndicator != null) _nextIndicator.SetActive(false);

            // 1. Fade out current card if visible
            if (_cardCanvasGroup != null && _cardCanvasGroup.alpha > 0.01f)
            {
                yield return Tween.Alpha(_cardCanvasGroup, 0f, _cardFadeDuration, Ease.InOutQuad).ToYieldInstruction();
            }

            // 2. Setup next card content
            if (_uiImage != null)
            {
                if (card.Image != null)
                {
                    _uiImage.gameObject.SetActive(true);
                    _uiImage.sprite = card.Image;
                }
                else
                {
                    _uiImage.gameObject.SetActive(false);
                }
            }

            _currentFullText = card.NarrationText;
            _uiText.text = "";
            _isTextFullyTyped = false;

            // 3. Fade in canvas group
            if (_cardCanvasGroup != null)
            {
                yield return Tween.Alpha(_cardCanvasGroup, 1f, _cardFadeDuration, Ease.InOutQuad).ToYieldInstruction();
            }

            _isTransitioning = false;

            // 4. Start typewriter effect
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(_currentFullText));
        }

        private IEnumerator TypewriterRoutine(string text)
        {
            _uiText.text = "";
            for (int i = 0; i <= text.Length; i++)
            {
                _uiText.text = text.Substring(0, i);
                yield return new WaitForSeconds(_typewriterSpeed);
            }
            _isTextFullyTyped = true;
            if (_nextIndicator != null) _nextIndicator.SetActive(true);
        }

        private void CompleteTypewriter()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }
            _uiText.text = _currentFullText;
            _isTextFullyTyped = true;
            if (_nextIndicator != null) _nextIndicator.SetActive(true);
        }

        private void GoToNextScene()
        {
            _isTransitioning = true;
            if (ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.TransitionToScene(_nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(_nextSceneName);
            }
        }
    }
}
