using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PrimeTween;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Manages the scrolling credits sequence.
    /// Scrolls credits text from bottom to top and returns to the main menu when finished or skipped.
    /// </summary>
    public sealed class CreditsScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _scrollContent;
        [SerializeField] private TextMeshProUGUI _creditsText;
        
        [Header("Settings")]
        [SerializeField] private float _scrollDuration = 16f;
        [SerializeField] private string _mainMenuSceneName = "mainmenu";

        public RectTransform ScrollContent => _scrollContent;
        public TextMeshProUGUI CreditsText => _creditsText;
        public float ScrollDuration => _scrollDuration;

        private Tween _scrollTween;
        private bool _isSkipping;

        private void Start()
        {
            // Reset TimeScale
            Time.timeScale = 1f;

            if (_scrollContent == null)
            {
                Debug.LogError("[Credits] Scroll Content RectTransform is not assigned!");
                return;
            }

            // Start off-screen at the bottom
            // Using a standard reference resolution offset
            _scrollContent.anchoredPosition = new Vector2(0f, -600f);

            // Animate to scroll off-screen at the top
            _scrollTween = Tween.UIAnchoredPosition(_scrollContent, new Vector2(0f, 1500f), _scrollDuration, Ease.Linear)
                .OnComplete(ReturnToMainMenu);
        }

        private void Update()
        {
            // Allow skipping via Escape, Space, Enter or left mouse click
            // Explicitly qualify UnityEngine.Input to avoid collision with Game.Runtime.Input namespace
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape) || 
                UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) || 
                UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return) || 
                UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (!_isSkipping)
                {
                    _isSkipping = true;
                    _scrollTween.Stop();
                    ReturnToMainMenu();
                }
            }
        }

        private void ReturnToMainMenu()
        {
            if (ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.TransitionToScene(_mainMenuSceneName);
            }
            else
            {
                SceneManager.LoadScene(_mainMenuSceneName);
            }
        }

        private void OnDestroy()
        {
            _scrollTween.Stop();
        }
    }
}
