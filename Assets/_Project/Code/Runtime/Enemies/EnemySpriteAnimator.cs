using UnityEngine;

namespace Game.Runtime.Enemies
{
    /// <summary>
    /// Plays an <see cref="EnemyDefinition"/>'s frame lists on the enemy's sprite, picking walk or
    /// idle from <see cref="EnemyController.IsMoving"/>. The frames live on the archetype asset, so
    /// a new enemy stays "a new asset, not a new class" — this component never learns their names.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemySpriteAnimator : MonoBehaviour
    {
        [Tooltip("Owner of the archetype. Auto-resolved from the parent when left empty.")]
        [SerializeField] private EnemyController _controller;
        [SerializeField] private SpriteRenderer _renderer;

        private Sprite[] _current;
        private float _timer;
        private int _frame;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_controller == null) _controller = GetComponentInParent<EnemyController>();
        }

        private void Update()
        {
            var definition = _controller != null ? _controller.Definition : null;
            if (definition == null) return;

            // Attack wins over walk wins over idle.
            bool attacking = _controller.IsAttacking && definition.CanAttack;
            var frames = attacking ? definition.AttackFrames
                       : _controller.IsMoving ? definition.WalkFrames
                       : definition.IdleFrames;
            if (frames == null || frames.Length == 0) return;

            // Restart cleanly when swapping clips so a short clip can't index past its end.
            if (!ReferenceEquals(frames, _current))
            {
                _current = frames;
                _frame = 0;
                _timer = 0f;
                _renderer.sprite = frames[0];
            }

            if (frames.Length == 1 || definition.FramesPerSecond <= 0f) return;

            _timer += Time.deltaTime;
            float frameDuration = 1f / definition.FramesPerSecond;
            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;

                // The attack plays once and holds its last frame; the controller ends the state on
                // a timer, so looping here would restart the swing and desync from the damage tick.
                if (attacking)
                {
                    if (_frame < frames.Length - 1) _frame++;
                }
                else
                {
                    _frame = (_frame + 1) % frames.Length;
                }

                _renderer.sprite = frames[_frame];
            }
        }
    }
}
