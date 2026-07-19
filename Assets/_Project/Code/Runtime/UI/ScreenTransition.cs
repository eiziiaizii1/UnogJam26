using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Persistent screen fade transition manager using PrimeTween. Fades to solid color, 
    /// loads scene asynchronously, then fades back to transparent.
    /// </summary>
    public sealed class ScreenTransition : MonoBehaviour
    {
        public static ScreenTransition Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fadeImage;

        [Header("Settings")]
        [SerializeField] private float _fadeDuration = 0.4f;
        [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

        private bool _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Initiates a smooth fade transition to the target scene.
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _isTransitioning = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                // Fade to solid color
                yield return Tween.Alpha(_canvasGroup, 1f, _fadeDuration, _fadeEase).ToYieldInstruction();
            }

            // Load Target Scene
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

            // Small delay for rendering to settle
            yield return new WaitForEndOfFrame();

            if (_canvasGroup != null)
            {
                // Fade back to transparent
                yield return Tween.Alpha(_canvasGroup, 0f, _fadeDuration, _fadeEase).ToYieldInstruction();
                _canvasGroup.blocksRaycasts = false;
            }

            _isTransitioning = false;
        }
    }
}
