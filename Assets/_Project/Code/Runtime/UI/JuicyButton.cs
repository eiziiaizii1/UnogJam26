using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PrimeTween;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Premium button juice component.
    /// Animates buttons on hover (scale up with spring bounce) and on press (shrink).
    /// Hooks to central SfxPlayer to trigger hover/click SFX.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Settings")]
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _pressScale = 0.94f;
        [SerializeField] private float _animationDuration = 0.15f;

        [Header("Audio (Optional)")]
        [SerializeField] private Audio.SfxDefinition _hoverSfx;
        [SerializeField] private Audio.SfxDefinition _clickSfx;

        private Vector3 _originalScale;
        private Tween _scaleTween;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
            if (_button != null) _button.onClick.AddListener(PlayClickSfx);
        }

        private void OnDisable()
        {
            _scaleTween.Stop();
            if (_button != null) _button.onClick.RemoveListener(PlayClickSfx);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            
            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, _originalScale * _hoverScale, _animationDuration, Ease.OutBack);

            // Play hover sound using central SfxPlayer
            if (_hoverSfx != null && Audio.SfxPlayer.Instance != null)
            {
                Audio.SfxPlayer.Instance.Play(_hoverSfx);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, _originalScale, _animationDuration, Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            _scaleTween.Stop();
            _scaleTween = Tween.Scale(transform, _originalScale * _pressScale, 0.08f, Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            _scaleTween.Stop();
            // If pointer is still over the button, return to hover scale, else original
            Vector3 target = RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, eventData.position, eventData.pressEventCamera)
                ? _originalScale * _hoverScale
                : _originalScale;
            _scaleTween = Tween.Scale(transform, target, _animationDuration, Ease.OutQuad);
        }

        private void PlayClickSfx()
        {
            if (_clickSfx != null && Audio.SfxPlayer.Instance != null)
            {
                Audio.SfxPlayer.Instance.Play(_clickSfx);
            }
        }
    }
}
