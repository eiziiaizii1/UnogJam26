using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// A pooled projectile (Guide §4.2). Flies straight, then returns itself to its pool on
    /// lifetime expiry or first hit. Fully resets on <see cref="Launch"/> so a stale state from
    /// a previous life can never leak (the classic pooling heisenbug).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] private float _lifetimeSeconds = 2f;

        private Rigidbody2D _body;
        private IObjectPool<Bullet> _pool;
        private GameObject _owner;
        private float _despawnTime;
        private bool _released;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Called once by the pool when the instance is first created.</summary>
        public void SetPool(IObjectPool<Bullet> pool)
        {
            _pool = pool;
        }

        /// <summary>Resets and fires the bullet. <paramref name="owner"/> is ignored on hit so it cannot self-destruct.</summary>
        public void Launch(Vector2 position, Vector2 direction, float speed, GameObject owner)
        {
            _owner = owner;
            _released = false;
            _despawnTime = Time.time + _lifetimeSeconds;

            transform.position = position;
            _body.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            if (Time.time >= _despawnTime)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;
            Despawn();
        }

        private void Despawn()
        {
            if (_released) return;
            _released = true;
            _body.linearVelocity = Vector2.zero;

            if (_pool != null)
            {
                _pool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
