using UnityEngine;
using Game.Runtime.Input;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Lightweight, allocation-free player 2D sprite animator.
    /// Manages walking loop, idle sprite choice, and 3-stage jump/fall rendering based on dikey velocity.
    /// Handles left/right facing directions preserving asymmetric artwork.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader _input;
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Appearance")]
        [Tooltip("Look applied at Awake. RunController overrides this per level; the inline arrays " +
                 "below are only used when no appearance is assigned.")]
        [SerializeField] private PlayerAppearance _appearance;
        
        [Header("Idle Sprites")]
        [Tooltip("Looping idle. The current art is front-facing, so both directions can share it.")]
        [SerializeField] private Sprite[] _idleRight;
        [SerializeField] private Sprite[] _idleLeft;
        [Tooltip("Slower than the walk cycle — idle is a breath, not a step.")]
        [SerializeField] private float _idleFrameDuration = 0.45f;

        [Header("Walk Sprites")]
        [SerializeField] private Sprite[] _walkRight;
        [SerializeField] private Sprite[] _walkLeft;
        [SerializeField] private float _walkFrameDuration = 0.12f;

        [Header("Jump/Fall Sprites")]
        [Tooltip("Right facing jump: Index 0=ascending, 1=apex, 2=descending")]
        [SerializeField] private Sprite[] _jumpRight;
        [Tooltip("Left facing jump: Index 0=ascending, 1=apex, 2=descending")]
        [SerializeField] private Sprite[] _jumpLeft;

        [Header("Grounding")]
        [Tooltip("A contact counts as ground when its normal.y is at least this (1 = flat floor).")]
        [SerializeField] private float _minGroundNormalY = 0.5f;

        private Rigidbody2D _body;
        
        private bool _isGrounded;
        private float _animationTimer;
        private int _currentFrame;
        private bool _facingRight = true;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            if (_input == null)
            {
                _input = GetComponent<InputReader>();
            }

            if (_appearance != null) ApplyAppearance(_appearance);
        }

        /// <summary>
        /// Swaps every clip to a different robot look. Safe to call at runtime — the frame index is
        /// reset because the new look may have fewer frames than the current one (robot 1 walks in
        /// 2, robot 2 in 3), and a stale index would read past the end.
        /// </summary>
        public void ApplyAppearance(PlayerAppearance appearance)
        {
            if (appearance == null) return;

            _appearance = appearance;
            _idleRight = appearance.IdleRight;
            _idleLeft = appearance.IdleLeft;
            _walkRight = appearance.WalkRight;
            _walkLeft = appearance.WalkLeft;
            _jumpRight = appearance.JumpRight;
            _jumpLeft = appearance.JumpLeft;

            _currentFrame = 0;
            _animationTimer = 0f;

            // Show the new look immediately rather than waiting for the next Update, so the swap
            // can't be seen as a one-frame flash of the old robot.
            var idle = _facingRight ? _idleRight : _idleLeft;
            if (_renderer != null && idle != null && idle.Length > 0) _renderer.sprite = idle[0];
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= _minGroundNormalY)
                {
                    _isGrounded = true;
                    return;
                }
            }
        }

        private void FixedUpdate()
        {
            // Reset grounded flag; OnCollisionStay2D will re-assert it if still touching ground
            _isGrounded = false;
        }

        private void Update()
        {
            UpdateFacing();
            UpdateAnimation();
        }

        private void UpdateFacing()
        {
            if (_input == null) return;

            float moveX = _input.Move.x;
            if (moveX > 0.01f)
            {
                _facingRight = true;
            }
            else if (moveX < -0.01f)
            {
                _facingRight = false;
            }
        }

        private void UpdateAnimation()
        {
            if (_isGrounded)
            {
                float moveX = _input != null ? Mathf.Abs(_input.Move.x) : 0f;
                if (moveX > 0.01f)
                {
                    // Walking
                    PlayLoopAnimation(_facingRight ? _walkRight : _walkLeft, _walkFrameDuration);
                }
                else
                {
                    // Idle
                    PlayLoopAnimation(_facingRight ? _idleRight : _idleLeft, _idleFrameDuration);
                }
            }
            else
            {
                // In Air (Jumping or Falling)
                Sprite[] jumpSprites = _facingRight ? _jumpRight : _jumpLeft;
                if (jumpSprites != null && jumpSprites.Length > 0)
                {
                    float yVelocity = _body.linearVelocity.y;
                    int index = 1; // Default to Peak / Apex frame

                    if (yVelocity > 1f)
                    {
                        index = 0; // Rising
                    }
                    else if (yVelocity < -1f)
                    {
                        index = Mathf.Min(2, jumpSprites.Length - 1); // Falling
                    }

                    index = Mathf.Clamp(index, 0, jumpSprites.Length - 1);
                    _renderer.sprite = jumpSprites[index];
                }
            }
        }

        private void PlayLoopAnimation(Sprite[] frames, float frameDuration)
        {
            if (frames == null || frames.Length == 0) return;

            _animationTimer += Time.deltaTime;
            if (_animationTimer >= frameDuration)
            {
                _animationTimer -= frameDuration;
                _currentFrame = (_currentFrame + 1) % frames.Length;
            }

            _currentFrame = Mathf.Clamp(_currentFrame, 0, frames.Length - 1);
            _renderer.sprite = frames[_currentFrame];
        }
    }
}
