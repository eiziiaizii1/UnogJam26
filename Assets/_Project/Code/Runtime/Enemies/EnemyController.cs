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

        private Rigidbody2D _body;
        private HealthComponent _health;
        private SpriteRenderer _renderer;
        private float _originX;
        private int _direction = 1;

        /// <summary>Sets the archetype before Awake (used by a spawner/factory).</summary>
        public void Configure(EnemyDefinition definition) => _definition = definition;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<HealthComponent>();
            _renderer = GetComponent<SpriteRenderer>();
            _originX = transform.position.x;
            ApplyDefinition();
        }

        private void ApplyDefinition()
        {
            if (_definition == null) return;

            _health.SetMaxHealth(_definition.MaxHealth);
            if (_renderer != null) _renderer.color = _definition.TintColor;
            transform.localScale = new Vector3(_definition.Size.x, _definition.Size.y, 1f);

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
            if (half <= 0f || speed <= 0f) return;

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

            _body.MovePosition(new Vector2(nextX, position.y));
        }

        private void SetDirection(int direction)
        {
            _direction = direction;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
    }
}
