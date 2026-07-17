using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Despawns the object when its <see cref="HealthComponent"/> dies. Reused by destructibles
    /// now and enemies later (composition over a per-type death script). Spawning debris / VFX
    /// on death is a later (M4 juice) concern that hangs off the same Died event.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public sealed class DestroyOnDeath : MonoBehaviour
    {
        [SerializeField] private float _destroyDelaySeconds = 0f;

        private HealthComponent _health;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        private void OnEnable()
        {
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Died -= HandleDied;
        }

        private void HandleDied()
        {
            Destroy(gameObject, _destroyDelaySeconds);
        }
    }
}
