using Game.Runtime.Events;
using UnityEngine;
using PrimeTween;

namespace Game.Runtime.Collectibles
{
    /// <summary>
    /// A nature pickup. When the player's <see cref="PlayerCollector"/> overlaps it, it announces
    /// its value on the shared channel and despawns — it never touches the score directly (§5.4),
    /// so HUD, audio, and achievements can all listen to the same fact independently.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Collectible : MonoBehaviour
    {
        [SerializeField] private int _value = 1;
        [SerializeField] private IntEventChannel _pickedChannel;

        [Header("Bob")]
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2f;

        private Vector3 _basePosition;
        private bool _collected;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
            transform.position = _basePosition + new Vector3(0f, offset, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (other.GetComponentInParent<PlayerCollector>() == null) return;

            _collected = true;
            if (_pickedChannel != null) _pickedChannel.Raise(_value);

            // Disable components to prevent further interaction or bobbing
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            enabled = false;

            // Animate scale-down and alpha-fade
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Tween.Alpha(sr, 0f, 0.2f);
            }
            Tween.Scale(transform, Vector3.zero, 0.2f, Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
