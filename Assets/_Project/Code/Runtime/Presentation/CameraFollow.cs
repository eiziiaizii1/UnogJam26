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

        /// <summary>Assigns the transform to follow (used by the sandbox builder / spawners).</summary>
        public void SetTarget(Transform target) => _target = target;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 current = transform.position;
            Vector3 desired = _target.position + _offset;

            float x = Mathf.SmoothDamp(current.x, desired.x, ref _velocity.x, _horizontalSmoothTime);
            float y = Mathf.SmoothDamp(current.y, desired.y, ref _velocity.y, _verticalSmoothTime);

            transform.position = new Vector3(x, y, desired.z);
        }
    }
}
