using Game.Runtime.Combat;
using UnityEngine;

namespace Game.Runtime.Enemies
{
    /// <summary>
    /// Marries an <see cref="EnemyDefinition"/> to behaviour: applies the archetype's stats to the
    /// shared components (health, look) at spawn, then patrols back and forth. Movement only —
    /// health lives in <see cref="HealthComponent"/> and death in <see cref="DestroyOnDeath"/>
    /// (composition, §3.2). A generic wave factory is deferred to M3; for now the definition is a
    /// serialized reference on the prefab.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [Tooltip("Child that carries the sprite. Sized and flipped instead of the root, so the collider " +
                 "keeps its authored shape. Falls back to this object for the old square-sprite prefab.")]
        [SerializeField] private Transform _visuals;

        private Rigidbody2D _body;
        private HealthComponent _health;
        private SpriteRenderer _renderer;
        private float _originX;
        private float _originY;
        private int _direction = 1;

        /// <summary>The archetype this enemy is running, for views (e.g. the sprite animator) to read.</summary>
        public EnemyDefinition Definition => _definition;

        /// <summary>True while the patrol is actually moving it, so the animator can pick walk vs idle.</summary>
        public bool IsMoving { get; private set; }

        /// <summary>Facing: +1 right, -1 left.</summary>
        public int Facing => _direction;

        /// <summary>Sets the archetype before Awake (used by a spawner/factory).</summary>
        public void Configure(EnemyDefinition definition) => _definition = definition;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<HealthComponent>();
            if (_visuals == null) _visuals = transform;
            _renderer = _visuals.GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
            _originX = transform.position.x;
            _originY = transform.position.y;
            ApplyDefinition();
        }

        private void ApplyDefinition()
        {
            if (_definition == null) return;

            _health.SetMaxHealth(_definition.MaxHealth);
            if (_renderer != null) _renderer.color = _definition.TintColor;
            _visuals.localScale = new Vector3(_definition.Size.x, _definition.Size.y, 1f);

            if (TryGetComponent<ContactDamage>(out var contactDamage))
            {
                contactDamage.SetDamage(_definition.ContactDamage);
            }
        }

        private void FixedUpdate()
        {
            if (_definition == null) return;

            float half = _definition.PatrolHalfWidth;
            float speed = _definition.MoveSpeed;
            if (half <= 0f || speed <= 0f)
            {
                IsMoving = false;
                // A stationary flyer should still bob, otherwise it reads as a floating statue.
                if (_definition.Flying) _body.MovePosition(new Vector2(_body.position.x, HoverY()));
                return;
            }

            Vector2 position = _body.position;
            float nextX = position.x + _direction * speed * Time.fixedDeltaTime;

            if (nextX >= _originX + half)
            {
                nextX = _originX + half;
                SetDirection(-1);
            }
            else if (nextX <= _originX - half)
            {
                nextX = _originX - half;
                SetDirection(1);
            }

            float nextY = _definition.Flying ? HoverY() : position.y;
            IsMoving = true;
            _body.MovePosition(new Vector2(nextX, nextY));
        }

        /// <summary>Sine bob around the spawn height. Flyers only — walkers keep whatever Y physics gave them.</summary>
        private float HoverY()
        {
            return _originY + Mathf.Sin(Time.time * _definition.HoverFrequency * Mathf.PI * 2f) * _definition.HoverAmplitude;
        }

        private void SetDirection(int direction)
        {
            _direction = direction;
            Vector3 scale = _visuals.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            _visuals.localScale = scale;
        }
    }
}
