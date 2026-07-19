using Game.Core.Data;
using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// One complete look for the robot — every clip <see cref="PlayerSpriteAnimator"/> needs, in one
    /// asset. A new robot for a new level is a new asset, not a new class or a prefab variant
    /// (Guide §5.5, the same rule <see cref="Game.Runtime.Enemies.EnemyDefinition"/> follows).
    /// <para>
    /// Frame counts may differ between looks — robot 1 walks in 2 frames, robot 2 in 3 — so the
    /// animator must never assume a length.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Player Appearance", fileName = "PlayerAppearance")]
    public sealed class PlayerAppearance : ScriptableObject, IValidatable
    {
        [SerializeField] private string _displayName = "Robot";

        [Header("Idle")]
        [SerializeField] private Sprite[] _idleRight;
        [SerializeField] private Sprite[] _idleLeft;

        [Header("Walk")]
        [SerializeField] private Sprite[] _walkRight;
        [SerializeField] private Sprite[] _walkLeft;

        [Header("Jump / Fall (0 = rising, 1 = apex, 2 = falling)")]
        [SerializeField] private Sprite[] _jumpRight;
        [SerializeField] private Sprite[] _jumpLeft;

        public string DisplayName => _displayName;
        public Sprite[] IdleRight => _idleRight;
        public Sprite[] IdleLeft => _idleLeft;
        public Sprite[] WalkRight => _walkRight;
        public Sprite[] WalkLeft => _walkLeft;
        public Sprite[] JumpRight => _jumpRight;
        public Sprite[] JumpLeft => _jumpLeft;

        public bool Validate(out string error)
        {
            // Every clip is required: a half-filled appearance shows up as the robot vanishing
            // mid-jump, which is far harder to diagnose than a validation error at boot.
            if (!Check(_idleRight, nameof(IdleRight), out error)) return false;
            if (!Check(_idleLeft, nameof(IdleLeft), out error)) return false;
            if (!Check(_walkRight, nameof(WalkRight), out error)) return false;
            if (!Check(_walkLeft, nameof(WalkLeft), out error)) return false;
            if (!Check(_jumpRight, nameof(JumpRight), out error)) return false;
            if (!Check(_jumpLeft, nameof(JumpLeft), out error)) return false;

            error = null;
            return true;
        }

        private bool Check(Sprite[] frames, string label, out string error)
        {
            if (frames == null || frames.Length == 0)
            {
                error = $"{name}: {label} has no frames.";
                return false;
            }

            foreach (var frame in frames)
            {
                if (frame == null)
                {
                    error = $"{name}: {label} contains an empty slot.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
