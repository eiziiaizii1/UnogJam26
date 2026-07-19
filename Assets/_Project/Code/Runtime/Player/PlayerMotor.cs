using Game.Runtime.Input;
using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Walk + jump for the robot (Guide §11.3, §11.7). A thin adapter: reads intents from
    /// <see cref="InputReader"/> and drives a <see cref="Rigidbody2D"/> in FixedUpdate. Grounding
    /// is collision-normal based (no ground LayerMask to configure). Feel niceties — coyote time,
    /// jump buffering, variable jump height — are cheap and tunable in the inspector.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader _input;

        [Header("Walk")]
        [SerializeField] private float _walkSpeedUnitsPerSecond = 6f;
        [SerializeField] private float _groundAccelerationUnitsPerSecondSq = 60f;
        [SerializeField] private float _airAccelerationUnitsPerSecondSq = 30f;

        [Header("Jump")]
        [SerializeField] private float _jumpSpeedUnitsPerSecond = 12f;
        [Tooltip("Upward velocity is multiplied by this when the jump button is released early (variable height).")]
        [SerializeField] private float _jumpCutMultiplier = 0.5f;
        [SerializeField] private float _coyoteTimeSeconds = 0.1f;
        [SerializeField] private float _jumpBufferSeconds = 0.1f;

        [Header("Grounding")]
        [Tooltip("A contact counts as ground when its normal.y is at least this (1 = flat floor).")]
        [SerializeField] private float _minGroundNormalY = 0.5f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem _footstepDust;

        private Rigidbody2D _body;
        private float _lastGroundedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;

        private bool IsGrounded => Time.time - _lastGroundedTime <= _coyoteTimeSeconds;
        private bool JumpBuffered => Time.time - _lastJumpPressedTime <= _jumpBufferSeconds;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.freezeRotation = true;
        }

        /// <summary>Applied by upgrades — additive on top of the authored base values.</summary>
        public void AddSpeedBonuses(float walkBonus, float jumpBonus)
        {
            _walkSpeedUnitsPerSecond += Mathf.Max(0f, walkBonus);
            _jumpSpeedUnitsPerSecond += Mathf.Max(0f, jumpBonus);
        }

        private void OnEnable()
        {
            if (_input == null) return;
            _input.JumpPressed += OnJumpPressed;
            _input.JumpReleased += OnJumpReleased;
        }

        private void OnDisable()
        {
            if (_input == null) return;
            _input.JumpPressed -= OnJumpPressed;
            _input.JumpReleased -= OnJumpReleased;
        }

        private void FixedUpdate()
        {
            float moveX = _input != null ? _input.Move.x : 0f;
            ApplyHorizontal(moveX);
            TryConsumeBufferedJump();
            UpdateDustEmission();
        }

        private void ApplyHorizontal(float moveInput)
        {
            float targetSpeed = moveInput * _walkSpeedUnitsPerSecond;
            float acceleration = IsGrounded ? _groundAccelerationUnitsPerSecondSq : _airAccelerationUnitsPerSecondSq;
            float newX = Mathf.MoveTowards(_body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
            _body.linearVelocity = new Vector2(newX, _body.linearVelocity.y);
        }

        private void TryConsumeBufferedJump()
        {
            if (!JumpBuffered || !IsGrounded) return;

            _body.linearVelocity = new Vector2(_body.linearVelocity.x, _jumpSpeedUnitsPerSecond);
            _lastJumpPressedTime = float.NegativeInfinity; // consume the buffered press
            _lastGroundedTime = float.NegativeInfinity;    // leave the ground immediately (no double jump)

            // Jump burst effect
            if (_footstepDust != null)
            {
                _footstepDust.Emit(6);
            }
        }

        private void UpdateDustEmission()
        {
            if (_footstepDust == null) return;
            var emission = _footstepDust.emission;
            bool shouldEmit = IsGrounded && Mathf.Abs(_body.linearVelocity.x) > 0.2f && _input != null && Mathf.Abs(_input.Move.x) > 0.1f;
            if (emission.enabled != shouldEmit)
            {
                emission.enabled = shouldEmit;
            }
        }

        private void OnJumpPressed()
        {
            _lastJumpPressedTime = Time.time;
        }

        private void OnJumpReleased()
        {
            if (_body.linearVelocity.y > 0f)
            {
                _body.linearVelocity = new Vector2(_body.linearVelocity.x, _body.linearVelocity.y * _jumpCutMultiplier);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            bool wasGrounded = IsGrounded;
            bool foundGround = false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= _minGroundNormalY)
                {
                    _lastGroundedTime = Time.time;
                    foundGround = true;
                    break;
                }
            }

            // Landing burst effect
            if (foundGround && !wasGrounded && _footstepDust != null)
            {
                _footstepDust.Emit(5);
            }
        }
    }
}
