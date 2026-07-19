using UnityEngine;
using PrimeTween;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Premium UI panel juice component.
    /// Animates UI panels with a slow yoyo floating (vertical) and swaying (rotational) 
    /// loop to simulate wind/movement and add depth.
    /// </summary>
    public sealed class JuicyPanel : MonoBehaviour
    {
        [Header("Floating Settings")]
        [SerializeField] private float _floatDistance = 10f;
        [SerializeField] private float _floatDuration = 2.8f;
        [SerializeField] private Ease _floatEase = Ease.InOutQuad;

        [Header("Swaying Settings")]
        [SerializeField] private float _swayAngle = 1.2f;
        [SerializeField] private float _swayDuration = 3.4f;
        [SerializeField] private Ease _swayEase = Ease.InOutQuad;

        private Tween _floatTween;
        private Tween _swayTween;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;

            // Float loop (Y position)
            _floatTween = Tween.LocalPositionY(
                transform, 
                _originalPosition.y - _floatDistance / 2f, 
                _originalPosition.y + _floatDistance / 2f, 
                _floatDuration, 
                _floatEase, 
                cycles: -1, 
                cycleMode: CycleMode.Yoyo
            );

            // Sway loop (Z rotation)
            _swayTween = Tween.LocalRotation(
                transform, 
                Quaternion.Euler(0f, 0f, -_swayAngle), 
                Quaternion.Euler(0f, 0f, _swayAngle), 
                _swayDuration, 
                _swayEase, 
                cycles: -1, 
                cycleMode: CycleMode.Yoyo
            );
        }

        private void OnDisable()
        {
            _floatTween.Stop();
            _swayTween.Stop();
        }
    }
}
