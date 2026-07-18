using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// Factory-fronted pool for <see cref="ImpactFlash"/> bursts, built the same way as
    /// <see cref="Game.Runtime.Combat.BulletPool"/>. Exposes a static <see cref="Instance"/>
    /// (the <see cref="Game.Runtime.Audio.SfxPlayer"/> pattern) because the callers are pooled
    /// prefabs that cannot hold a scene reference.
    /// <para>Optional by design: with no instance in the scene, nothing spawns and nothing breaks.</para>
    /// </summary>
    public sealed class ImpactFlashPool : MonoBehaviour
    {
        public static ImpactFlashPool Instance { get; private set; }

        [SerializeField] private ImpactFlash _prefab;
        [SerializeField] private int _prewarmCount = 8;
        [SerializeField] private int _maxSize = 32;

        private IObjectPool<ImpactFlash> _pool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _pool = new ObjectPool<ImpactFlash>(
                createFunc: CreateFlash,
                actionOnGet: flash => flash.gameObject.SetActive(true),
                actionOnRelease: flash => flash.gameObject.SetActive(false),
                actionOnDestroy: flash => Destroy(flash.gameObject),
                collectionCheck: false,
                defaultCapacity: _prewarmCount,
                maxSize: _maxSize);

            Prewarm();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Plays a burst at a world position. The only method callers need.</summary>
        public ImpactFlash Spawn(Vector2 position)
        {
            if (_prefab == null) return null;

            var flash = _pool.Get();
            flash.Play(position);
            return flash;
        }

        private ImpactFlash CreateFlash()
        {
            var flash = Instantiate(_prefab, transform);
            flash.SetPool(_pool);
            return flash;
        }

        private void Prewarm()
        {
            if (_prefab == null) return;

            var warmed = new ImpactFlash[_prewarmCount];
            for (int i = 0; i < _prewarmCount; i++) warmed[i] = _pool.Get();
            for (int i = 0; i < _prewarmCount; i++) _pool.Release(warmed[i]);
        }
    }
}
