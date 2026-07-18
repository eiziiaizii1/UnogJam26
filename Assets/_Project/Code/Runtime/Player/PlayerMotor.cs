using Game.Runtime.Input;
using UnityEngine;
using PrimeTween;

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
        [Tooltip("Esneme/Yaylanma animasyonunun uygulanacağı görsel nesne. Eğer idle animasyonu scale değerini kilitliyorsa (Animator yüzünden), Animator bulunmayan boş bir üst container nesnesini buraya atayın.")]
        [SerializeField] private Transform _spriteParent;

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

        [Header("Gamefeel & Squish/Stretch Settings")]
        [Tooltip("Zıplama anındaki ölçek çarpanı (X = Daralma, Y = Uzama). Robot karakterler için hafif tutulması önerilir (örneğin X=0.96, Y=1.05).")]
        [SerializeField] private Vector2 _jumpScaleMultiplier = new Vector2(0.96f, 1.05f);
        [SerializeField] private float _jumpStretchDuration = 0.12f;
        [Tooltip("Yere iniş anındaki ölçek çarpanı (X = Yayılma, Y = Basılma).")]
        [SerializeField] private Vector2 _landScaleMultiplier = new Vector2(1.04f, 0.95f);
        [SerializeField] private float _landSquishDuration = 0.1f;

        private Rigidbody2D _body;
        private float _lastGroundedTime = float.NegativeInfinity;
        private float _lastJumpPressedTime = float.NegativeInfinity;

        // Gamefeel / Squish & Stretch variables
        private bool _previouslyGrounded;
        private Vector3 _initialScale = Vector3.one;
        private Sequence _squishStretchTween;

        private bool IsGrounded => Time.time - _lastGroundedTime <= _coyoteTimeSeconds;
        private bool JumpBuffered => Time.time - _lastJumpPressedTime <= _jumpBufferSeconds;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.freezeRotation = true;
            _previouslyGrounded = true;

            Transform targetSprite = _spriteParent != null ? _spriteParent : transform;
            _initialScale = targetSprite.localScale;
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
            // Grounded state transition check for landing
            bool isGroundedNow = IsGrounded;
            if (isGroundedNow && !_previouslyGrounded)
            {
                TriggerLandSquish();
            }
            _previouslyGrounded = isGroundedNow;

            float moveX = _input != null ? _input.Move.x : 0f;
            ApplyHorizontal(moveX);
            TryConsumeBufferedJump();
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

            TriggerJumpStretch();
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
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= _minGroundNormalY)
                {
                    _lastGroundedTime = Time.time;
                    return;
                }
            }
        }

        private void TriggerJumpStretch()
        {
            Transform targetSprite = _spriteParent != null ? _spriteParent : transform;
            _squishStretchTween.Stop();
            targetSprite.localScale = _initialScale;
            
            Vector3 targetScale = new Vector3(_initialScale.x * _jumpScaleMultiplier.x, _initialScale.y * _jumpScaleMultiplier.y, _initialScale.z);
            _squishStretchTween = Tween.Scale(targetSprite, targetScale, _jumpStretchDuration, ease: Ease.OutQuad)
                .Chain(Tween.Scale(targetSprite, _initialScale, _jumpStretchDuration * 1.2f, ease: Ease.InOutQuad));
        }

        private void TriggerLandSquish()
        {
            Transform targetSprite = _spriteParent != null ? _spriteParent : transform;
            _squishStretchTween.Stop();
            targetSprite.localScale = _initialScale;

            Vector3 targetScale = new Vector3(_initialScale.x * _landScaleMultiplier.x, _initialScale.y * _landScaleMultiplier.y, _initialScale.z);
            _squishStretchTween = Tween.Scale(targetSprite, targetScale, _landSquishDuration, ease: Ease.OutQuad)
                .Chain(Tween.Scale(targetSprite, _initialScale, _landSquishDuration * 1.5f, ease: Ease.InOutQuad));
        }
    }
}
