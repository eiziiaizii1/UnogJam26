using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Spawns loot when this object's <see cref="HealthComponent"/> dies. Deliberately generic —
    /// it knows nothing about enemies, so the same component drops pickups from a shot tree or a
    /// crate (composition over a per-type death script, like <see cref="DestroyOnDeath"/>).
    /// <para>
    /// Drops are spawned <b>unparented</b>. <see cref="DestroyOnDeath"/> shrinks the corpse to zero
    /// before destroying it, and a child would be scaled away with it.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class DropOnDeath : MonoBehaviour
    {
        [Header("Loot")]
        [SerializeField] private GameObject _dropPrefab;
        [Tooltip("Inclusive range. Both at 1 drops exactly one.")]
        [SerializeField] private int _minCount = 1;
        [SerializeField] private int _maxCount = 1;
        [Range(0f, 1f)]
        [Tooltip("Chance to drop anything at all. 1 = always.")]
        [SerializeField] private float _dropChance = 1f;

        [Header("Placement")]
        [Tooltip("Offset from this object's origin, before scatter.")]
        [SerializeField] private Vector2 _spawnOffset = new(0f, 0.25f);
        [Tooltip("Random horizontal spread so a multi-drop doesn't stack into one sprite.")]
        [SerializeField] private float _scatterRadius = 0.45f;

        [Header("Ground snap")]
        [Tooltip("Drop loot onto the surface below instead of at the spawn height. Objects sit at " +
                 "wildly different heights — trees are seated into the grass by varying amounts and " +
                 "some stand on platforms — so a fixed offset buries some drops and floats others.")]
        [SerializeField] private bool _snapToGround = true;
        [Tooltip("Ray starts this far ABOVE the spawn point, so loot from an object seated into the " +
                 "ground still finds the surface it's sunk beneath.")]
        [SerializeField] private float _rayStartHeight = 1.5f;
        [SerializeField] private float _rayMaxDistance = 6f;
        [Tooltip("Gap between the surface and the pickup's centre — roughly half the sprite.")]
        [SerializeField] private float _groundClearance = 0.3f;

        private HealthComponent _health;
        private bool _dropped;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        private void OnEnable() => _health.Died += HandleDied;

        private void OnDisable() => _health.Died -= HandleDied;

        private void HandleDied()
        {
            // Died can be raised again by a second killing blow landing in the same frame.
            if (_dropped || _dropPrefab == null) return;
            _dropped = true;

            if (Random.value > _dropChance) return;

            int count = Random.Range(_minCount, _maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 scatter = count > 1
                    ? new Vector2(Random.Range(-_scatterRadius, _scatterRadius), 0f)
                    : Vector2.zero;

                Vector3 position = transform.position + (Vector3)(_spawnOffset + scatter);
                if (_snapToGround) position.y = FindSurfaceY(position, position.y);

                // Position via Instantiate rather than after: Collectible caches its bob origin in
                // Start, so a later move would leave it bobbing around the wrong point.
                Instantiate(_dropPrefab, position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Y of the first solid surface below <paramref name="from"/>, plus clearance. Falls back to
        /// <paramref name="fallbackY"/> when nothing is hit, so loot over a pit still spawns.
        /// </summary>
        private float FindSurfaceY(Vector2 from, float fallbackY)
        {
            Vector2 origin = from + Vector2.up * _rayStartHeight;
            var hits = Physics2D.RaycastAll(origin, Vector2.down, _rayMaxDistance);

            // RaycastAll returns hits in distance order, so the first valid one is the surface.
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.isTrigger) continue;                        // pickups and the exit aren't floor
                if (hit.collider.transform.IsChildOf(transform)) continue;   // our own corpse
                return hit.point.y + _groundClearance;
            }

            return fallbackY;
        }
    }
}
