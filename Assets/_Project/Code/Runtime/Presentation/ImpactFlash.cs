using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Presentation
{
    /// <summary>
    /// A pooled one-shot burst played where a bullet lands. Mirrors
    /// <see cref="Game.Runtime.Combat.Bullet"/>'s lifecycle: fully reset on <see cref="Play"/>,
    /// returns itself to the pool when its lifetime expires.
    /// <para>
    /// The visuals live on the prefab (a <see cref="LightPulse"/>, optionally particles) — this
    /// component only owns placement and despawn, so the effect can be re-art-directed in the
    /// editor without touching code.
    /// </para>
    /// </summary>
    public sealed class ImpactFlash : MonoBehaviour
    {
        [Tooltip("Should outlast the LightPulse envelope on this prefab, or the burst gets cut off.")]
        [SerializeField] private float _lifetimeSeconds = 0.25f;

        private IObjectPool<ImpactFlash> _pool;
        private float _despawnTime;
        private bool _released;

        /// <summary>Called once by the pool when the instance is first created.</summary>
        public void SetPool(IObjectPool<ImpactFlash> pool)
        {
            _pool = pool;
        }

        /// <summary>Places and starts the burst. Unscaled so a hit-stop doesn't stall it.</summary>
        public void Play(Vector2 position)
        {
            transform.position = position;
            _released = false;
            _despawnTime = Time.unscaledTime + _lifetimeSeconds;
        }

        private void Update()
        {
            if (Time.unscaledTime >= _despawnTime) Despawn();
        }

        private void Despawn()
        {
            if (_released) return;
            _released = true;

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
