using UnityEngine;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// Smoothly follows a target in LateUpdate (Guide §11.7 — cameras/followers run late so
    /// they read the fully-updated, interpolated target position). Horizontal and vertical
    /// smoothing are separate: a side-scroller usually wants snappy horizontal tracking but a
    /// softer vertical response so jumps don't jerk the camera.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Framing")]
        [Tooltip("Added to the target position. Keep Z negative so the camera stays behind the scene.")]
        [SerializeField] private Vector3 _offset = new(0f, 1f, -10f);

        [Header("Smoothing (approx. seconds to catch up)")]
        [SerializeField] private float _horizontalSmoothTime = 0.15f;
        [SerializeField] private float _verticalSmoothTime = 0.30f;

        private Vector3 _velocity;

        // Screen shake fields
        private Vector3 _shakeOffset;
        private float _shakeIntensity;
        private float _shakeDecay;

        /// <summary>Assigns the transform to follow (used by the sandbox builder / spawners).</summary>
        public void SetTarget(Transform target) => _target = target;

        /// <summary>Starts a screen shake effect with given intensity and decay rate.</summary>
        public void Shake(float intensity, float decay = 5f)
        {
            _shakeIntensity = intensity;
            _shakeDecay = decay;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 current = transform.position;
            // Subtract previous shake offset so the camera follow path calculation doesn't drift
            current -= _shakeOffset;

            Vector3 desired = _target.position + _offset;

            float x = Mathf.SmoothDamp(current.x, desired.x, ref _velocity.x, _horizontalSmoothTime);
            float y = Mathf.SmoothDamp(current.y, desired.y, ref _velocity.y, _verticalSmoothTime);

            // Update screen shake offset
            if (_shakeIntensity > 0.01f)
            {
                _shakeOffset = Random.insideUnitSphere * _shakeIntensity;
                _shakeOffset.z = 0f; // Keep Z axis intact for 2D perspective
                _shakeIntensity = Mathf.MoveTowards(_shakeIntensity, 0f, _shakeDecay * Time.deltaTime);
            }
            else
            {
                _shakeOffset = Vector3.zero;
            }

            transform.position = new Vector3(x, y, desired.z) + _shakeOffset;
        }
    }
}
