using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// The factory-fronted object pool for bullets (Guide §4.1, §4.2). Call sites use
    /// <see cref="Spawn"/> and never learn that pooling exists. Pre-warms so the first burst of
    /// fire allocates nothing.
    /// </summary>
    public sealed class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet _prefab;
        [SerializeField] private int _prewarmCount = 32;
        [SerializeField] private int _maxSize = 256;

        private IObjectPool<Bullet> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<Bullet>(
                createFunc: CreateBullet,
                actionOnGet: bullet => bullet.gameObject.SetActive(true),
                actionOnRelease: bullet => bullet.gameObject.SetActive(false),
                actionOnDestroy: bullet => Destroy(bullet.gameObject),
                collectionCheck: false,
                defaultCapacity: _prewarmCount,
                maxSize: _maxSize);

            Prewarm();
        }

        /// <summary>Fires a bullet from the pool. The only method callers need.</summary>
        public Bullet Spawn(Vector2 position, Vector2 direction, float speed, GameObject owner)
        {
            var bullet = _pool.Get();
            bullet.Launch(position, direction, speed, owner);
            return bullet;
        }

        private Bullet CreateBullet()
        {
            var bullet = Instantiate(_prefab, transform);
            bullet.SetPool(_pool);
            return bullet;
        }

        private void Prewarm()
        {
            var warmed = new Bullet[_prewarmCount];
            for (int i = 0; i < _prewarmCount; i++) warmed[i] = _pool.Get();
            for (int i = 0; i < _prewarmCount; i++) _pool.Release(warmed[i]);
        }
    }
}
