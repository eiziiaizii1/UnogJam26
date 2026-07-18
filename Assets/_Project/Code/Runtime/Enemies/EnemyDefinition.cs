using Game.Core.Data;
using UnityEngine;

namespace Game.Runtime.Enemies
{
    /// <summary>
    /// Authored enemy archetype (Guide §4.1 Flyweight, §11.4). Adding enemy #2, #3, #4 is a new
    /// asset, not a new class (Pillar 4). Implements <see cref="IValidatable"/> so the boot/menu
    /// data validator names a broken archetype instead of null-reffing mid-playtest (§3.6).
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject, IValidatable
    {
        [SerializeField] private string _displayName = "Enemy";
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private int _contactDamage = 1;
        [Tooltip("Half the width of the back-and-forth patrol, in world units, centred on spawn.")]
        [SerializeField] private float _patrolHalfWidth = 3f;
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField] private Vector2 _size = Vector2.one;

        [Header("Animation (leave empty for a static sprite)")]
        [Tooltip("Played while standing still. Frames come straight off the sliced sheet.")]
        [SerializeField] private Sprite[] _idleFrames;
        [Tooltip("Played while patrolling. Falls back to the idle frames when empty.")]
        [SerializeField] private Sprite[] _walkFrames;
        [SerializeField] private float _framesPerSecond = 8f;

        [Header("Movement")]
        [Tooltip("Flyers ignore the ground and bob around their spawn height instead of walking on it.")]
        [SerializeField] private bool _flying;
        [SerializeField] private float _hoverAmplitude = 0.5f;
        [SerializeField] private float _hoverFrequency = 1.5f;

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public int ContactDamage => _contactDamage;
        public float PatrolHalfWidth => _patrolHalfWidth;
        public Color TintColor => _tintColor;
        public Vector2 Size => _size;

        public Sprite[] IdleFrames => _idleFrames;

        /// <summary>Walk frames, or the idle frames when a sheet has no separate walk row.</summary>
        public Sprite[] WalkFrames => _walkFrames != null && _walkFrames.Length > 0 ? _walkFrames : _idleFrames;

        public float FramesPerSecond => _framesPerSecond;
        public bool Flying => _flying;
        public float HoverAmplitude => _hoverAmplitude;
        public float HoverFrequency => _hoverFrequency;

        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1, _maxHealth);
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _contactDamage = Mathf.Max(0, _contactDamage);
            _patrolHalfWidth = Mathf.Max(0f, _patrolHalfWidth);
            _framesPerSecond = Mathf.Max(0f, _framesPerSecond);
            _hoverAmplitude = Mathf.Max(0f, _hoverAmplitude);
            _hoverFrequency = Mathf.Max(0f, _hoverFrequency);
        }

        public bool Validate(out string error)
        {
            if (_maxHealth <= 0) { error = $"{name}: MaxHealth must be positive."; return false; }
            if (_moveSpeed < 0f) { error = $"{name}: MoveSpeed must be non-negative."; return false; }
            if (_contactDamage < 0) { error = $"{name}: ContactDamage must be non-negative."; return false; }
            if (_patrolHalfWidth < 0f) { error = $"{name}: PatrolHalfWidth must be non-negative."; return false; }

            // A null slot in a frame array is the classic "dragged the sheet, missed a cell" mistake:
            // it renders as a one-frame blink mid-animation, which is maddening to track down at runtime.
            if (HasNullFrame(_idleFrames)) { error = $"{name}: IdleFrames contains an empty slot."; return false; }
            if (HasNullFrame(_walkFrames)) { error = $"{name}: WalkFrames contains an empty slot."; return false; }
            if (_framesPerSecond <= 0f && _idleFrames != null && _idleFrames.Length > 1)
            { error = $"{name}: FramesPerSecond must be positive to animate."; return false; }

            error = null;
            return true;
        }

        private static bool HasNullFrame(Sprite[] frames)
        {
            if (frames == null) return false;
            foreach (var frame in frames) if (frame == null) return true;
            return false;
        }
    }
}
