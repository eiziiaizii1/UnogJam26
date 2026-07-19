using UnityEngine;
using PrimeTween;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Despawns the object when its <see cref="HealthComponent"/> dies. Reused by destructibles
    /// now and enemies later (composition over a per-type death script). Plays a juicy scale-down
    /// and fade-out animation before destruction using PrimeTween.
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
            // 1. Disable colliders immediately to prevent further collision/bullet hits
            var colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 2. Play juicy death animations
            float animDuration = Mathf.Max(0.35f, _destroyDelaySeconds);

            // Scale down to 0 with spring-out bounce
            Tween.Scale(transform, Vector3.zero, animDuration, Ease.InBack);

            // Fade out the sprite renderer if one exists
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                Color targetColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
                Tween.Color(spriteRenderer, spriteRenderer.color, targetColor, animDuration, Ease.InQuad);
            }

            // 3. Destroy object after animation completes
            Tween.Delay(animDuration).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
